using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Реализован командный режим на 2 и более игроков
/// <summary> Все методы должны выполняться только на сервере, с клиента можно вызывать [ CompareAnswer(string guess), CompareMonster(string guess) ] </summary>
public class GameManager : NetworkBehaviour
{
    #region Fields

    [SerializeField] private Guesser guesser;
    [SerializeField] private Bestiary bestiary;
    
    private Painter painter;
    [SerializeField] private CardManager cardManager;

    // temp
    private int roundCount = 4;
    private int currentRound = 1;
    // temp

    private CardSO answerCardSO;
    private int currentIngredientIndex;

    private NetworkVariable<ushort> painterId = new(ushort.MaxValue);
    private List<ulong> lastPainterIds = new();
    public bool IsPainter => painterId.Value == NetworkManager.Singleton.LocalClientId;

    public static GameManager Instance { get; private set; }

    [HideInInspector] public UnityEvent OnCorrectIngredientGuess; // При верном предположении ингредиента
    [HideInInspector] public UnityEvent OnWrongIngredientGuess; // При неверном предположении ингредиента
    [HideInInspector] public UnityEvent OnIngredientsEnd; // Когда закончились ингредиенты на карточке

    [HideInInspector] public UnityEvent OnWinRound; // Когда монстр отгадан
    [HideInInspector] public UnityEvent OnLoseRound; // Когда монстр не отгадан

    [HideInInspector] public UnityEvent OnEndGame; // Все монстры отгаданы

    #endregion
    #region Initialization

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            LogWarning($"Two GameManagers on scene:\n{Instance}, {this}");
        
        if (bestiary == null)
            bestiary = FindObjectOfType<Bestiary>();
        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();
        if (painter == null)
            painter = FindObjectOfType<Painter>();
        if (guesser == null)
            guesser = FindObjectOfType<Guesser>();

        cardManager.OnChooseCard.AddListener(SetAnswerCardSO);
        Timer.Instance.OnExpired.AddListener(LoseRound);

        bestiary.OnChooseMonster.AddListener(CompareMonster);
    }

    public override void OnNetworkSpawn()
    {
        painterId.OnValueChanged += PainterId_OnValueChanged;
        //cardManager.GetComponent<NetworkObject>().Spawn(true);

        Log($"IsServer : {IsServer}");

        if (IsServer)
        {
            painterId.Value = (ushort)NetworkManager.ConnectedClientsIds[0];
            OnIngredientsEnd.AddListener(ActivateGuessMonsterStageClientRpc);
        }
        else
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) => { SceneManager.LoadScene("Menu"); };
            StartCoroutine(ChooseRole());
        }
    }

    #endregion
    #region Roles

    private void PainterId_OnValueChanged(ushort previousValue, ushort newValue)
    {
        Log("PainterId_OnValueChanged");
        lastPainterIds.Add(newValue);
        StartCoroutine(ChooseRole());
    }

    private void SetPainter()
    {
        Log("Painter");

        //bestiary.gameObject.SetActive(false);

        guesser.Deactivate();
        painter.Activate();
    }

    private void SetGuesser()
    {
        Log("Guesser");

        //bestiary.gameObject.SetActive(false);
        cardManager.enabled = false;

        painter.Deactivate();
        guesser.Activate();
    }

    // не работает [ NetworkManager.Singleton.LocalClientId ], с IEnumerator почему-то работает
    private IEnumerator ChooseRole()
    {
        yield return new WaitForSeconds(0f);


        if (IsPainter)
            SetPainter();
        else
            SetGuesser();
    }

    private void ChangeRoles()
    {
        foreach (ushort clientId in NetworkManager.ConnectedClientsIds)
        {
            if (!lastPainterIds.Contains(clientId))
            {
                painterId.Value = clientId;
                return;
            }
        }

        // выполняется, если все сыграли за рисующего
        ClearLastPaintersClientRpc();
        painterId.Value = (ushort)NetworkManager.ConnectedClientsIds[0];
    }

    [ClientRpc]
    private void ClearLastPaintersClientRpc()
    {
        lastPainterIds.Clear();
    }

    #endregion
    #region Base

    private void CorrectIngredientGuess()
    {
        Log("Correct guess");

        OnCorrectIngredientGuess?.Invoke();
        NextIngredient();
    }

    private void WrongIngredientGuess()
    {
        Log("Wrong guess");
        OnWrongIngredientGuess?.Invoke();
    }

    private void NextIngredient()
    {
        currentIngredientIndex++;

        if (currentIngredientIndex >= answerCardSO.Ingredients.Length)
        {
            OnIngredientsEnd?.Invoke();
            return;
        }
    }

    [ClientRpc]
    private void ActivateGuessMonsterStageClientRpc()
    {
        if (!IsPainter)
        {
            //bestiary.gameObject.SetActive(true);
            guesser.SetMonsterGuessStage();
            //guesser.DeactivateUI();
        }
    }

    private void NextRound()
    {
        currentRound++;

        if (currentRound > roundCount)
        {
            EndGame();
            return;
        }
        ChangeRoles();
        Timer.Instance.ResetToDefault();
    }

    private void WinRound()
    {
        /* Условие: Угадали монстра
         * Действия:
         *  - Раздача токенов
         *  - Смена ролей
         *  - Ресет таймера
         *  - Выбор новым рисовальщиком монстра
         */

        Log("WinRound");
        AddTokenClientRpc();
        NextRound();
        OnWinRound?.Invoke();
        // сообщить игрокам о смен раунда
    }

    [ClientRpc]
    private void AddTokenClientRpc()
    {
        TokensManager.AddTokens(1);
    }

    private void LoseRound()
    {
        /* Условие: Не угадали монстра
         * Действия:
         *  - Смена ролей
         *  - Ресет таймера
         *  - Выбор новым рисовальщиком монстра
         */

        Log("LoseRound");
        NextRound();
        OnLoseRound?.Invoke();
        // сообщить игрокам о смен раунда
    }



    private void EndGame()
    {
        /* Условие: Монстры закончились (угаданы/не угаданы)
         * Действия:
         *  - Раздача монет (внутриигровая валюта)
         *  - Выход в лобби
         */

        OnEndGame?.Invoke();
        NetworkManager.Singleton.DisconnectClient(1);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("Menu");
        // Сообщить всем игрокам о выигрыше/проигрыше
    }

    #endregion
    #region Compare

    [ServerRpc (RequireOwnership = false)]
    private void CompareIngredientServerRpc(FixedString32Bytes guess, ServerRpcParams serverRpcParams)
    {
        string stringGuess = guess.ToString();
        GuessHistory.Instance.AddGuess(serverRpcParams.Receive.SenderClientId, stringGuess);

        Log($"Current Ingredient: {answerCardSO.Ingredients[currentIngredientIndex]}, Guess: {stringGuess}");

        if (answerCardSO.Ingredients[currentIngredientIndex] == stringGuess.ToLower())
            CorrectIngredientGuess();
        else
            WrongIngredientGuess();
    }

    [ServerRpc (RequireOwnership = false)]
    private void CompareMonsterServerRpc(ushort guessCardSOIndex)
    {
        if (guessCardSOIndex == ushort.MaxValue)
        {
            LoseRound();
            return;
        }

        CardSO guessCardSO = cardManager.GetCardSOByIndex(guessCardSOIndex);

        Log($"Current Monster: {answerCardSO.Id}, Guess: {guessCardSO.Id}");

        if (answerCardSO == guessCardSO)
            WinRound();
        else
            LoseRound();
    }

    public void CompareIngredient(string guess)
    {
        FixedString32Bytes fixedStringGuess = new(guess);
        ServerRpcParams serverRpcParams = new();

        CompareIngredientServerRpc(fixedStringGuess, serverRpcParams);
    }

    public void CompareMonster(CardSO guessCardSO)
    {
        ushort guessCardSOIndex = ushort.MaxValue;
        
        if (guessCardSO)
            guessCardSOIndex = cardManager.GetCardSOIndex(guessCardSO);
        
        Log("Compare Monster");

        CompareMonsterServerRpc(guessCardSOIndex);
    }

    #endregion
    #region CardSO

    private void OnCardSOUpdate()
    {
        Log("CardSO Updated");
        currentIngredientIndex = 0;
        Timer.Instance.StartTimerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCardSOServerRpc(ushort cardSOIndex)
    {
        answerCardSO = cardManager.GetCardSOByIndex(cardSOIndex);
        OnCardSOUpdate();
    }

    private void SetAnswerCardSO(ushort cardSOIndex)
    {
        SetCardSOServerRpc(cardSOIndex);
    }

    #endregion
    #region Logs

    private void Log(string message) => Debug.Log("[GameManager] " + message);
    private void LogWarning(string message) => Debug.LogWarning("[GameManager] " + message);
    private void LogError(string message) => Debug.LogError("[GameManager] " + message);
    
    #endregion
}