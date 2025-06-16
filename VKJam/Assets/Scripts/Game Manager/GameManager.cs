using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

// TODO:
// Add second mode guess system to scene

public class GameManager : NetworkBehaviour, IGameManager
{
    public static GameManager Instance { get; private set; }

    public UnityEvent OnRoundStartedOnClient { get; private set; } = new();
    public UnityEvent OnIngredientGuessStartedOnClient { get; private set; } = new();
    public UnityEvent<BaseCardSO> OnCardChoosedOnClient { get; private set; } = new();
    public UnityEvent<int> OnIngredientSwitchedOnClient { get; private set; } = new();
    /// <summary> Sends true if local player is painter, false if not </summary>
    public UnityEvent<bool> OnGuessMonsterStageActivatedOnClient { get; private set; } = new();
    public UnityEvent OnGameEnded { get; private set; } = new();

    [SerializeField] private GameConfigSO gameConfig;
    [SerializeField] private MainGameTimer timer;

    [Header("Managers")]
    [SerializeField] private RoleManager roleManager;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private PlayersStatusManager playersStatusManager;
    [SerializeField] private SceneObjectsManager sceneObjectsManager;
    [SerializeField] private HintManager hintManager;
    [Space(10)]
    [SerializeField] private FirstModeGuessSystem firstModeGuessSystem;
    [SerializeField] private SecondModeGuessSystem secondModeGuessSystem;
    [SerializeField] private ReadinessSystem readinessSystem;

    [Header("Bestiary")]
    [SerializeField] private Bestiary bestiary;
    [SerializeField] private BestiaryIngredients bestiaryIngredients;

    [Header("Paint")]
    [SerializeField] private DrawingMultiplayer paint;

    [Header("Buttons")]
    [SerializeField] private Button nextRoundButton;
    [SerializeField] private string nextRoundButtonText;
    [SerializeField] private string returnToLobbyButtonText;

    [Header("Scene Monster")]
    [SerializeField] private GameObject sceneMonster;
    [SerializeField] private Material sceneMonsterMaterial;
    [SerializeField] private Texture hiddenMonsterTexture;
    private Animator sceneMonsterAnimator;

    [Header("Sounds")]
    [SerializeField] SoundList soundList;

    [Header("Default game objects root")]
    [SerializeField] private GameObject rootObject;

    private IngredientManager ingredientManager;
    private RoundManager roundManager;

    private int currentRound = 1;
    private int roundAmount = 4;
    private bool isGameEnded;

    private BaseCardSO answerCardInfo;

    #region Properties

    public Stage Stage { get; private set; } = Stage.Waiting;
    public bool IsSecondMode { get; private set; }
    public bool IsTeamMode { get; private set; }
    public int CurrentRound => currentRound;

    public bool IsDangerousCard => answerCardInfo.Difficulty == CardDifficulty.Dangerous;
    public string CurrentMonsterName => answerCardInfo.Id;

    public string CurrentIngredientName => answerCardInfo.Ingredients[ingredientManager.GetCurrentIngredientIndex];
    public int IngredientsCount => answerCardInfo.Ingredients.Length;

    public IGuessSystem GuessSystem => IsSecondMode ? secondModeGuessSystem : firstModeGuessSystem;
    public DrawingMultiplayer Paint => paint;
    public IngredientManager IngredientManager => ingredientManager;
    public RoundManager RoundManager => roundManager;
    public RoleManager RoleManager => roleManager;
    public CardManager CardManager => cardManager;
    public GameObject SceneMonster => sceneMonster;
    public Animator SceneMonsterAnimator => sceneMonsterAnimator;
    public SoundList SoundList => soundList;
    public SceneObjectsManager SceneObjectsManager => sceneObjectsManager;
    
    public GameObject RootObject => rootObject;
    
    public byte PainterId => roleManager.PainterId;
    public bool IsPainter => roleManager.IsPainter;

    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            LogError($"Two GameManagers on scene:\n{Instance}, {this}");
            return;
        }

        sceneMonsterMaterial = sceneMonster.GetComponent<Renderer>().material;
        sceneMonsterAnimator = sceneMonster.GetComponent<Animator>();

        cardManager.OnChooseCard.AddListener(SetAnswerCardSO);
    }

    public override void OnNetworkSpawn()
    {
        Debug.LogWarning("ASDASDASDAS");
        StartCoroutine(TakePack());
        timer.Init(gameConfig);

        IsSecondMode = LobbyManager.Instance.CurrentLobby.Data[LobbyManager.KEY_SECOND_MODE].Value == "True";
        IsTeamMode = LobbyManager.Instance.CurrentLobby.Data[LobbyManager.KEY_TEAM_MODE].Value == "True";

        if (IsSecondMode || !IsTeamMode)
        {
            roleManager.OnGuesserSetted.AddListener(() => playersStatusManager.SetActive(false));
            roleManager.OnPainterSetted.AddListener(() => playersStatusManager.SetActive(true));

            playersStatusManager.SetActive(IsServer);
        }

        roleManager.OnPainterSetted.AddListener(() => BackgroundMusic.Instance.Play("tokens_take-card"));
        roleManager.OnGuesserSetted.AddListener(() => BackgroundMusic.Instance.Play("default"));

        if (IsServer)
        {
            timer.OnExpired.AddListener(OnTimeExpired);

            if (IsSecondMode)
            {
                ingredientManager = new SecondModeIngredientManager(this, gameConfig, secondModeGuessSystem);
                roundManager = new SecondModeRoundManager(this, gameConfig, secondModeGuessSystem, ingredientManager);
            }
            else
            {
                if (IsTeamMode)
                {
                    Log("TEAM MODE");

                    ingredientManager = new TeamIngredientManager(this, gameConfig, firstModeGuessSystem);
                    roundManager = new TeamRoundManager(this, gameConfig, firstModeGuessSystem, ingredientManager);
                }
                else
                {
                    Log("COMPETITIVE MODE");

                    ingredientManager = new CompetitiveIngredientManager(this, gameConfig, firstModeGuessSystem);
                    roundManager = new CompetitiveRoundManager(this, gameConfig, firstModeGuessSystem, ingredientManager);
                }
            }

            roundManager.OnMonsterGuessed.AddListener(OnMonsterGuessedClientRpc);
            roundManager.OnMonsterNotGuessed.AddListener(OnMonsterNotGuessedClientRpc);

            ingredientManager.OnIngredientsEnded.AddListener(ActivateGuessMonsterStage);
            ingredientManager.OnIngredientSwitched.AddListener(OnIngredientSwitched);
            roundManager.OnRoundEnded.AddListener(OnRoundEnded);

            roundAmount = NetworkManager.Singleton.ConnectedClientsIds.Count;

            RelayManager.Instance.OnClientConnected.AddListener(OnClientConnected);
            RelayManager.Instance.OnClientDisconnect.AddListener(OnClientDisconnect);

            SetNRBRoles();
            nextRoundButton.gameObject.SetActive(true);
        }
        else
        {
            nextRoundButton.gameObject.SetActive(false);
        }

        if (IsSecondMode)
            rootObject.SetActive(false);
    }

    // server side
    private void OnClientConnected(ulong clientId)
    {
        if (isGameEnded)
            return;

        roundAmount++;
    }

    // server side
    private void OnClientDisconnect(ulong clientId)
    {
        if (isGameEnded)
            return;

        roundAmount--;

        if (PainterId == clientId)
            nextRoundButton.onClick?.Invoke();

        Log($"Client disconnected. Remaining clients: {NetworkManager.ConnectedClientsIds.Count}");
        if (NetworkManager.ConnectedClientsIds.Count <= 2)
        {
            Log($"Not enough clients to continue game. Try to end game");
            OnRoundEnded();
        }
    }

    private IEnumerator TakePack()
    {
        yield return new WaitForSeconds(0.1f);
        bestiary.TakePack();

        if (!IsSecondMode)
            bestiaryIngredients.TakePack();
    }

    // server side
    private void SetNRBRoles()
    {
        nextRoundButton.onClick.RemoveAllListeners();

        nextRoundButton.onClick.AddListener(() =>
        {
            OnRoundStartedClientRpc();
            roleManager.ChangeRoles();
        });

        nextRoundButton.GetComponentInChildren<TextMeshProUGUI>().text = nextRoundButtonText;
    }

    // server side. with vk connected
    private void SetNRBLobbyWithInterstitialAd(bool isWatched)
    {
        SetNRBLobby();
        VK_Connect.Instance.OnInterstitialAdTryWatched -= SetNRBLobbyWithInterstitialAd;
    }

    // server side
    private void SetNRBLobby()
    {
        nextRoundButton.gameObject.SetActive(true);
        nextRoundButton.onClick.RemoveAllListeners();

        nextRoundButton.onClick.AddListener(ReturnToLobby);
        nextRoundButton.GetComponentInChildren<TextMeshProUGUI>().text = returnToLobbyButtonText;
    }

    // server side
    private void OnTimeExpired()
    {
        if (Stage == Stage.MonsterGuess)
        {
            foreach (byte clientId in roundManager.CorrectGuesserIds)
                ShowMonsterGuessedClientRpc(clientId);

            timer.SetIngredientGuessTime();
            roundManager.OnTimeExpired();
        }
        else
            ingredientManager.OnTimeExpired();
    }

    [ClientRpc]
    private void ShowMonsterGuessedClientRpc(byte clientId)
    {
        if (clientId == NetworkManager.LocalClientId && !IsSecondMode)
            NotificationSystem.Instance.SendLocal("Вы отгадали монстра!");
    }

    [ClientRpc]
    private void OnRoundStartedClientRpc()
    {
        StartCoroutine(TakePack());
        playersStatusManager.ResetStatuses();
        OnRoundStartedOnClient?.Invoke();
    }

    [ClientRpc]
    private void OnRoundEndedClientRpc()
    {
        Stage = Stage.Waiting;

        sceneMonsterMaterial.mainTexture = answerCardInfo.MonsterTexture;
        sceneObjectsManager.OnRoundEnded();
        hintManager.DisableHandHint();

        if (!IsSecondMode)
            TokenManager.AccrueTokens();

        BackgroundMusic.Instance.Play("tokens_take-card");

        answerCardInfo = null;

        readinessSystem.DisableVisual();
    }

    // server side
    private void OnRoundEnded()
    {
        timer.StopTimer();
        currentRound++;

        Stage = Stage.Waiting;
        OnRoundEndedClientRpc();

        if (currentRound > roundAmount)
            EndGame();
    }

    #region GuessMonsterStage

    [ClientRpc]
    private void ActivateGuessMonsterStageClientRpc()
    {
        Stage = Stage.MonsterGuess;

        if (IsPainter)
        {
            paint.Disable();
            if (paint.enabled)
                paint.GetComponent<Animator>().Play("NoteBookClose");

            hintManager.SetHintData(answerCardInfo.Id);
            hintManager.DisableHandHint();
        }
        else
        {
            sceneMonsterMaterial.mainTexture = hiddenMonsterTexture;
            SceneMonster.SetActive(true);
            SceneMonsterAnimator.Play("Idle");
        }

        readinessSystem.EnableVisual();

        playersStatusManager.ResetStatuses();
        BackgroundMusic.Instance.Play("monsterGuess");
        OnGuessMonsterStageActivatedOnClient?.Invoke(IsPainter);
    }

    // server side
    private void ActivateGuessMonsterStage()
    {
        Stage = Stage.MonsterGuess;
        paint.ClearCanvasGlobal();

        if (!IsSecondMode)
            bestiaryIngredients.gameObject.SetActive(false);

        readinessSystem.SetAllUnready();
        readinessSystem.OnAllReady += OnAllGuessedMonster;

        ActivateGuessMonsterStageClientRpc();

        timer.SetMonsterGuessTime();
        timer.StartTimer();
    }

    private void OnAllGuessedMonster()
    {
        readinessSystem.OnAllReady -= OnAllGuessedMonster;
        roundManager.OnTimeExpired();

    }

    [ClientRpc]
    private void OnMonsterGuessedClientRpc()
    {
        if (!IsSecondMode)
        {
            SceneMonster.SetActive(true);
            SceneMonsterAnimator.Play("Win");
        }
    }

    [ClientRpc]
    private void OnMonsterNotGuessedClientRpc()
    {
        if (!IsSecondMode)
        {
            SceneMonster.SetActive(true);
            SceneMonsterAnimator.Play("Loose");
        }
    }

    // server side
    public void AllPlayersGuessed()
    {
        timer.PrematureStop();
    }

    #endregion
    #region SetCardSO

    [ClientRpc]
    private void SetCardSOClientRpc(byte cardSOIndex)
    {
        answerCardInfo = cardManager.GetCardInfoByIndex(cardSOIndex);

        sceneMonsterMaterial.mainTexture = answerCardInfo.MonsterTexture;
        OnCardChoosedOnClient?.Invoke(answerCardInfo);
    }

    [ServerRpc(RequireOwnership = false)] // server side
    private void SetCardSOServerRpc(byte cardSOIndex)
    {
        answerCardInfo = cardManager.GetCardInfoByIndex(cardSOIndex);
        sceneMonsterMaterial.mainTexture = answerCardInfo.MonsterTexture;

        ingredientManager.SetIngredients(answerCardInfo.Ingredients.Length);

        SetCardSOClientRpc(cardSOIndex);

        SetHintDataClientRpc((sbyte)ingredientManager.GetCurrentIngredientIndex);

        //Stage = Stage.IngredientGuess;

        Log("New answer CardSO: " + answerCardInfo.Id);
    }

    private void SetAnswerCardSO(byte cardSOIndex)
    {
        answerCardInfo = cardManager.GetCardInfoByIndex(cardSOIndex);

        sceneMonsterMaterial.mainTexture = answerCardInfo.MonsterTexture;
        SceneMonster.SetActive(true);
        SceneMonsterAnimator.Play("Idle");

        SetCardSOServerRpc(cardSOIndex);
    }

    #endregion

    private void SetHintData(sbyte ingredientIndex)
    {
        if (!IsPainter)
            return;

        if (ingredientIndex < 0)
            hintManager.SetHintData(answerCardInfo.Id);
        else
            hintManager.SetHintData(answerCardInfo.Ingredients[ingredientIndex]);
    }

    [ClientRpc]
    private void SetHintDataClientRpc(sbyte ingredientIndex)
    {
        SetHintData(ingredientIndex);
    }

    [ClientRpc]
    private void OnIngredientSwitchedClientRpc(sbyte ingredientIndex)
    {
        SetHintData(ingredientIndex);
        playersStatusManager.ResetStatuses();

        OnIngredientSwitchedOnClient?.Invoke(ingredientIndex);
    }

    // server side
    private void OnIngredientSwitched(sbyte ingredientIndex)
    {
        timer.SetIngredientGuessTime();
        timer.StartTimer();
        paint.ClearCanvasGlobal();

        OnIngredientSwitchedClientRpc(ingredientIndex);
    }

    // server side
    private void EndGame()
    {
        isGameEnded = true;

        if (!IsTeamMode)
            TokenManager.OnCompetitiveGameEnd();

        EndGameClientRpc();

        if (Authentication.IsLoggedInThroughVK)
        {
            nextRoundButton.gameObject.SetActive(false);
            VK_Connect.Instance.OnInterstitialAdTryWatched += SetNRBLobbyWithInterstitialAd;
            StartCoroutine(VK_Connect.Instance.RequestShowInterstitialAd());
            ShowInterstitialAdOnGameEndedClientRpc();
        }
        else
            SetNRBLobby();
    }

    [ClientRpc]
    private void ShowInterstitialAdOnGameEndedClientRpc()
    {
        if (IsServer)
            return;

        StartCoroutine(VK_Connect.Instance.RequestShowInterstitialAd());
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        isGameEnded = true;
        
        if (!IsSecondMode)
        {
            StartCoroutine(TokenManager.OnGameEnded());

            if (!IsTeamMode)
            {
                ulong winnerClientId;
                int winnerClientTokens;
                (winnerClientId, winnerClientTokens) = TokenManager.GetClientIdWithMaxTokens();

                NotificationSystem.Instance.SendLocal("Выиграл " + PlayersDataManager.Instance.PlayersData[winnerClientId].Name + "со счетом " + winnerClientTokens);
            }

            TokenManager.ResetData();
        }

        OnGameEnded?.Invoke();
    }

    // server side
    private void ReturnToLobby()
    {
        RelayManager.Instance.ReturnToLobby();
    }

    [ServerRpc(RequireOwnership = false)] // server side
    private void StartGameServerRpc()
    {
        Stage = Stage.IngredientGuess;

        timer.SetIngredientGuessTime();
        timer.StartTimer();

        StartGameClientRpc();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        Stage = Stage.IngredientGuess;

        if (IsPainter)
            BackgroundMusic.Instance.Play("painting");
        else
        {
            if (IsDangerousCard)
                BackgroundMusic.Instance.Play("ingredientGuess120s");
            else
                BackgroundMusic.Instance.Play("ingredientGuess150s");
        }

        OnIngredientGuessStartedOnClient?.Invoke();
    }

    public void StartGame()
    {
        if (answerCardInfo == null || Stage != Stage.Waiting)
            return;

        Stage = Stage.IngredientGuess;

        StartGameServerRpc();
    }

    private void Log(object message) => Logger.Instance.Log(this, message);
    private void LogWarning(object message) => Logger.Instance.LogWarning(this, message);
    private void LogError(object message) => Logger.Instance.LogError(this, message);
}