using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Реализован командный режим на 2 и более игроков
/// <summary> Все методы должны выполняться только на сервере, с клиента можно вызывать [ CompareIngredient(string guess), CompareMonster(string guess) ] </summary>
public class GameManager_OLD : NetworkBehaviour
{
    #region Fields

    [SerializeField] private Paint paint;
    [SerializeField] private GuesserPainting guesserPaint;
    [SerializeField] private Interactable painterBook;
    [SerializeField] private GameObject guessMonsterStageUI;

    [SerializeField] private GameObject mainCards;
    [SerializeField] private Bestiary bestiary;
    [SerializeField] private GameObject guesserUI;
    [SerializeField] private GameObject tokensSummary;
    private GameObject nextRoundButton;
    [SerializeField] private GameObject gameSummary;
    private GameObject returnToLobbyButton;
    
    [SerializeField] private GameObject[] painterGameObjects;
    [SerializeField] private GameObject[] guesserGameObjects;

    [SerializeField] private CardManager cardManager;
    [SerializeField] private GameObject sceneMonster;
    [SerializeField] private Texture hiddenMonster;

    private Material sceneMonsterMaterial;

    [SerializeField] private HintManager hintManager;

    private int tokensPerRound = 2;
    private int tokensPerIngredient = 1;

    // temp
    private int currentRound = 1;
    private int roundAmount = 4;
    // temp

    private CardSO answerCardSO;
    private int currentIngredientIndex;

    private bool isMonsterStage;

    private NetworkVariable<ushort> painterId = new(ushort.MaxValue);
    private List<ulong> lastPainterIds = new();
    public bool IsPainter => painterId.Value == NetworkManager.Singleton.LocalClientId;

    public static GameManager_OLD Instance { get; private set; }

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

        sceneMonsterMaterial = sceneMonster.GetComponent<Renderer>().material;
        nextRoundButton = tokensSummary.GetComponentInChildren<Button>().gameObject;
        returnToLobbyButton = gameSummary.GetComponentInChildren<Button>().gameObject;

        cardManager.OnChooseCard.AddListener(SetAnswerCardSO);
    }

    public override void OnNetworkSpawn()
    {
        painterId.OnValueChanged += PainterId_OnValueChanged;

        Log($"IsServer : {IsServer}");

        if (IsServer)
        {
            nextRoundButton.gameObject.SetActive(true);
            returnToLobbyButton.gameObject.SetActive(true);

            painterId.Value = (ushort)NetworkManager.ConnectedClientsIds[0];
            Timer.Instance.OnExpired.AddListener(OnTimeExpired);
            
            roundAmount = int.Parse(LobbyManager.Instance.CurrentLobby.Data[LobbyManager.Instance.KEY_ROUND_AMOUNT].Value);
        }
        else
        {
            nextRoundButton.gameObject.SetActive(false);
            returnToLobbyButton.gameObject.SetActive(false);

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

        guesserPaint.gameObject.SetActive(true);
        cardManager.enabled = false;
        paint.SetActive(false);
        painterBook.SetInteractable(true);
    }

    // не работает [ NetworkManager.Singleton.LocalClientId ], с IEnumerator почему-то работает
    private IEnumerator ChooseRole()
    {
        yield return null;

        bestiary.gameObject.SetActive(false);
        tokensSummary.SetActive(false);
        sceneMonster.SetActive(false);
        
        //cardManager.ResetMonsterSprite();

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

    private void LoseIngredient()
    {
        Log("Lose Ingredient");
        RemoveTokenClientRpc(tokensPerIngredient);

        //NextIngredient();
    }

    private void CorrectIngredientGuess()
    {
        Log("Correct guess");
        AddTokenClientRpc(tokensPerIngredient);

        OnCorrectIngredientGuess?.Invoke();
        //NextIngredient();
    }

    private void WrongIngredientGuess()
    {
        Log("Wrong guess");
        RemoveTokenClientRpc(tokensPerIngredient);

        OnWrongIngredientGuess?.Invoke();
    }

    private void NextIngredient()
    {
        currentIngredientIndex++;
        Timer.Instance.StartServerRpc();
        paint.ClearCanvas();
        
        if (currentIngredientIndex >= answerCardSO.Ingredients.Length)
        {
            ActivateGuessMonsterStageClientRpc();
            isMonsterStage = true;

            OnIngredientsEnd?.Invoke();
            //hintManager.SetHintData(answerCardSO.Id);
            SetHintDataClientRpc(-1);

            return;
        }

        //hintManager.SetHintData(answerCardSO.Ingredients[currentIngredientIndex]);
        SetHintDataClientRpc((sbyte)currentIngredientIndex);
    }

    [ClientRpc]
    private void ActivateGuessMonsterStageClientRpc()
    {
        if (IsPainter)
        {
            guessMonsterStageUI.SetActive(true);
            painterBook.SetInteractable(false);
            //paint.SetActive(false);
            // выводить догадки с разделением по игрокам
        }
        else
        {
            guesserPaint.gameObject.SetActive(false);
            sceneMonster.SetActive(true);
            sceneMonsterMaterial.mainTexture = hiddenMonster;

            //bestiary.gameObject.SetActive(true);
            //guesserUI.SetActive(false);
        }
    }

    [ClientRpc]
    private void EndRoundClientRpc(byte cardIndex)
    {
        guessMonsterStageUI.SetActive(false);
        TokensManager.AccrueTokens();
        
        tokensSummary.SetActive(true);
        
        sceneMonster.gameObject.SetActive(true);
    }

    private void EndRound()
    {
        currentRound++;

        if (currentRound > roundAmount)
        {
            EndGame();
            return;
        }

        isMonsterStage = false;

        EndRoundClientRpc(cardManager.GetCardSOIndex(answerCardSO));

        Timer.Instance.StopServerRpc();
    }

    public void NextRound()
    {
        ChangeRoles();
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
        AddTokenClientRpc(tokensPerRound);

        EndRound();
        OnWinRound?.Invoke();
        // сообщить игрокам о смен раунда
    }

    [ClientRpc]
    private void AddTokenClientRpc(int count)
    {
        TokensManager.AddTokens(count);
    }

    [ClientRpc]
    private void RemoveTokenClientRpc(int count)
    {
        TokensManager.RemoveTokens(count);
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
        RemoveTokenClientRpc(tokensPerRound);

        EndRound();
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
        
        //SceneLoader.ServerLoad("Lobby"); // отображать итоги в лобби

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
        gameSummary.SetActive(true);
    }

    #endregion
    #region Compare
    
    private void CompareIngredient(string guess, ulong guesserId)
    {
        string stringGuess = guess.ToString();
        GuessHistory.Instance.AddGuess(guesserId, stringGuess);

        Log($"Current Ingredient: {answerCardSO.Ingredients[currentIngredientIndex]}, Guess: {stringGuess}, Guesser Id: {guesserId}");

        if (answerCardSO.Ingredients[currentIngredientIndex].ToLower() == stringGuess.ToLower())
            CorrectIngredientGuess();
        else
            WrongIngredientGuess();
    }

    private void CompareMonster(string guess, ulong guesserId)
    {
        Log($"Current Monster: {answerCardSO.Id}, Guess: {guess}, Guesser Id: {guesserId}");

        if (answerCardSO.Id == guess)
            WinRound();
        else
            LoseRound();
    }

    [ServerRpc (RequireOwnership = false)]
    public void CompareAnswerServerRpc(string guess, ServerRpcParams serverRpcParams)
    {
        if (isMonsterStage)
            CompareMonster(guess, serverRpcParams.Receive.SenderClientId);
        else
            CompareIngredient(guess, serverRpcParams.Receive.SenderClientId);
    }

    #endregion
    #region CardSO

    [ClientRpc]
    private void SetMonsterTextureClientRpc(byte cardIndex)
    {
        CardSO cardSO = cardManager.GetCardSOByIndex(cardIndex);

        sceneMonsterMaterial.mainTexture = cardSO.MonsterTexture;
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetCardSOServerRpc(byte cardSOIndex)
    {
        answerCardSO = cardManager.GetCardSOByIndex(cardSOIndex);
        
        currentIngredientIndex = 0;
        SetHintDataClientRpc((sbyte)currentIngredientIndex);
        SetMonsterTextureClientRpc(cardSOIndex);

        Timer.Instance.StartServerRpc();

        Log("New answer CardSO: " + answerCardSO.Id);
    }

    private void SetAnswerCardSO(byte cardSOIndex)
    {
        answerCardSO = cardManager.GetCardSOByIndex(cardSOIndex);
        sceneMonsterMaterial.mainTexture = answerCardSO.MonsterTexture;
        sceneMonster.SetActive(true);

        SetCardSOServerRpc(cardSOIndex);
    }

    #endregion

    [ClientRpc]
    private void SetHintDataClientRpc(sbyte ingredientIndex)
    {
        if (!IsPainter)
            return;

        if (ingredientIndex < 0)
            hintManager.SetHintData(answerCardSO.Id);
        else
        {
            currentIngredientIndex = ingredientIndex;
            hintManager.SetHintData(answerCardSO.Ingredients[currentIngredientIndex]);
        }
    }

    private void OnTimeExpired()
    {
        if (currentIngredientIndex >= answerCardSO.Ingredients.Length)
        {
            LoseRound();
        }
        else
        {
            LoseIngredient();
            NextIngredient();
        }
    }

    public void InteractRecipeHand()
    {
        if (hintManager.IsActiveHandHint)
        {
            hintManager.DisableHandHint();
        }
        else if (paint.enabled == false)
        {
            //hintManager.SetHintData(answerCardSO.Ingredients[currentIngredientIndex]);
            hintManager.EnableHandHint();
        }
    }

    public void ReturnToLobby()
    {
        RelayManager.Instance.ReturnToLobby();
    }

    public void SetGuesserUIActive(bool value)
    {
        guesserUI.SetActive(value);
    }

    #region Logs

    private void Log(string message) => Debug.Log("[GameManager] " + message);
    private void LogWarning(string message) => Debug.LogWarning("[GameManager] " + message);
    private void LogError(string message) => Debug.LogError("[GameManager] " + message);
    
    #endregion
}