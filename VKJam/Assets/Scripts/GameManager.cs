using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

// Реализован командный режим на 2 и более игроков
/// <summary> Все методы должны выполняться только на сервере, с клиента можно вызывать [ CompareAnswer(string guess), CompareMonster(string guess) ] </summary>
public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] painterGameObjects;
    [SerializeField] private GameObject[] guesserGameObjects;

    [SerializeField] private CardManager cardManager;
    [SerializeField] private int monstersPerGame = 4;

    private CardSO answerCardSO;
    private int currentIngredientIndex;

    private NetworkVariable<ushort> painterId = new(ushort.MaxValue);
    private List<ulong> lastPainterIds = new();

    public static GameManager Instance { get; private set; }

    [HideInInspector] public UnityEvent OnCorrectIngredientGuess;
    [HideInInspector] public UnityEvent OnWrongIngredientGuess;
    [HideInInspector] public UnityEvent OnIngredientsEnd;

    [HideInInspector] public UnityEvent OnWinRound;
    [HideInInspector] public UnityEvent OnLoseRound;

    [HideInInspector] public UnityEvent OnEndGame;

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
    }

    public override void OnNetworkSpawn()
    {
        painterId.OnValueChanged += PainterId_OnValueChanged;

        Log($"IsServer : {IsServer}");

        if (IsServer)
            painterId.Value = (ushort)NetworkManager.ConnectedClientsIds[0];
        else
            StartCoroutine(ChooseRole(painterId.Value));
    }

    private void PainterId_OnValueChanged(ushort previousValue, ushort newValue)
    {
        Log("PainterId_OnValueChanged");
        lastPainterIds.Add(newValue);
        StartCoroutine(ChooseRole(newValue));
    }

    private void SetPainter()
    {
        Log("Painter");
        foreach (GameObject obj in painterGameObjects)
            obj.SetActive(true);
        foreach (GameObject obj in guesserGameObjects)
            obj.SetActive(false);
    }
    private void SetGuesser()
    {
        Log("Guesser");
        foreach (GameObject obj in painterGameObjects)
            obj.SetActive(false);
        foreach (GameObject obj in guesserGameObjects)
            obj.SetActive(true);
    }
    
    #endregion

    // не работает [ NetworkManager.Singleton.LocalClientId ], с IEnumerator почему-то работает
    private IEnumerator ChooseRole(ushort clientId)
    {
        yield return new WaitForSeconds(0f);

        if (clientId == NetworkManager.Singleton.LocalClientId)
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

    #region Base

    private void NextRound()
    {
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
        TokensManager.AddTokens(1);
        NextRound();
        OnWinRound?.Invoke();
        // сообщить игрокам о смен раунда
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

        // Сообщить всем игрокам о выигрыше/проигрыше
    }

    #endregion

    private void NextIngredient()
    {
        if (currentIngredientIndex >= answerCardSO.Ingredients.Length)
        {
            OnIngredientsEnd?.Invoke();
            return;
        }

        currentIngredientIndex++;
    }

    #region Compare

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

    [ServerRpc (RequireOwnership = false)]
    private void CompareIngredientServerRpc(FixedString32Bytes guess, ServerRpcParams serverRpcParams)
    {
        string stringGuess = guess.ToString();
        GuessHistory.Instance.AddGuess(serverRpcParams.Receive.SenderClientId, stringGuess);

        if (answerCardSO.Ingredients[currentIngredientIndex] == stringGuess.ToLower())
            CorrectIngredientGuess();
        else
            WrongIngredientGuess();
    }

    [ServerRpc (RequireOwnership = false)]
    private void CompareMonsterServerRpc(ushort guessCardSOIndex)
    {
        CardSO guessCardSO = cardManager.GetCardSOByIndex(guessCardSOIndex);

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
        CompareMonsterServerRpc(cardManager.GetCardSOIndex(guessCardSO));
    }

    #endregion
    #region CardSO

    private void OnCardSOUpdate()
    {
        Log("CardSO Updated");
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