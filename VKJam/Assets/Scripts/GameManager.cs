using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Реализован командный режим на 2 и более игроков
/// <summary> Все методы должны выполняться только на сервере, с клиента можно вызывать [ CompareIngredient(string guess), CompareMonster(string guess) ] </summary>
public class GameManager : NetworkBehaviour
{
    #region Fields

    [SerializeField] private Paint paint;
    [SerializeField] private Bestiary bestiary;
    [SerializeField] private GameObject mainCards;
    [SerializeField] private GameObject guesserUI;

    [SerializeField] private GameObject[] painterGameObjects;
    [SerializeField] private GameObject[] guesserGameObjects;

    [SerializeField] private CardManager cardManager;

    [SerializeField] private Hint hint;

    // temp
    private int roundCount = 2;
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
        
        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();

        cardManager.OnChooseCard.AddListener(SetAnswerCardSO);
        Timer.Instance.OnExpired.AddListener(LoseRound);
        
        bestiary.OnChooseMonster.AddListener(CompareMonster);
    }

    public override void OnNetworkSpawn()
    {
        painterId.OnValueChanged += PainterId_OnValueChanged;

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
        foreach (GameObject obj in painterGameObjects)
            obj.SetActive(true);
        foreach (GameObject obj in guesserGameObjects)
            obj.SetActive(false);

        mainCards.SetActive(true);
        paint.ClearCanvas();
        paint.SetActive(true);
    }

    private void SetGuesser()
    {
        Log("Guesser");
        foreach (GameObject obj in painterGameObjects)
            obj.SetActive(false);
        foreach (GameObject obj in guesserGameObjects)
            obj.SetActive(true);

        cardManager.enabled = false;
        paint.SetActive(false);
    }

    // не работает [ NetworkManager.Singleton.LocalClientId ], с IEnumerator почему-то работает
    private IEnumerator ChooseRole()
    {
        yield return new WaitForSeconds(0f);

        bestiary.gameObject.SetActive(false);

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
        AddTokenClientRpc();
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
            ActivateGuessMonsterStageClientRpc();
            OnIngredientsEnd?.Invoke();
            return;
        }
    }

    [ClientRpc]
    private void ActivateGuessMonsterStageClientRpc()
    {
        if (IsPainter)
        {
            // выводить догадки с разделением по игрокам
        }
        else
        {
            bestiary.gameObject.SetActive(true);
            guesserUI.SetActive(false);
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
        //Timer.Instance.ResetToDefault();
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
        // сообщить игрокам о смене раунда
    }

    private void EndGame()
    {
        /* Условие: Монстры закончились (угаданы/не угаданы)
         * Действия:
         *  - Раздача монет (внутриигровая валюта)
         *  - Подведение итогов
         */

        OnEndGame?.Invoke();
        EndGameClientRpc();
        
        SceneLoader.ServerLoad("Lobby"); // отображать итоги в лобби

        //NetworkManager.Singleton.DisconnectClient(1);
        //NetworkManager.Singleton.Shutdown();
        //RelayService.Instance.
        //SceneManager.LoadScene("Menu");
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        // раздача монет
        // 
        
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
        ushort guessCardSOIndex = cardManager.GetCardSOIndex(guessCardSO);

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

    public void InteractRecipeHand()
    {

        if (hint.gameObject.activeInHierarchy)
        {
            hint.HideHint();
        }
        else
        {
            hint.SetHint(answerCardSO.Ingredients[currentIngredientIndex]);
        }
    }

    #region Logs

    private void Log(string message) => Debug.Log("[GameManager] " + message);
    private void LogWarning(string message) => Debug.LogWarning("[GameManager] " + message);
    private void LogError(string message) => Debug.LogError("[GameManager] " + message);
    
    #endregion
}