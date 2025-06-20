using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour, IGameManager
{
    public static GameManager Instance { get; private set; }

    [HideInInspector] public UnityEvent OnRoundStartedOnClient = new();
    [HideInInspector] public UnityEvent<CardSO> OnCardChoosedOnClient = new();
    [HideInInspector] public UnityEvent<int> OnIngredientSwitchedOnClient = new();
    /// <summary> Sends true if local player is painter, false if not </summary>
    [HideInInspector] public UnityEvent<bool> OnGuessMonsterStageActivatedOnClient = new();
    [HideInInspector] public UnityEvent OnGameEnded = new();

    [SerializeField] private GameConfigSO gameConfig;
    [SerializeField] private MainGameTimer timer;

    [Header("Managers")]
    [SerializeField] private RoleManager roleManager;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private FirstModeGuessSystem compareSystem;
    [SerializeField] private PlayersStatusManager playersStatusManager;
    [SerializeField] private SceneObjectsManager sceneObjectsManager;
    [SerializeField] private HintManager hintManager;

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

    private IngredientManager ingredientManager;
    private RoundManager roundManager;

    private int currentRound = 1;
    private int roundAmount = 4;
    private bool isGameEnded;

    private CardSO answerCardSO;

    #region Properties

    public Stage Stage { get; private set; } = Stage.Waiting;
    public bool IsSecondMode { get; private set; }
    public bool IsTeamMode { get; private set; }
    public int CurrentRound => currentRound;

    public bool IsDangerousCard => answerCardSO.Difficulty == CardDifficulty.Dangerous;
    public string CurrentMonsterName => GetCurrentMonster();
    
    public string CurrentIngredientName => GetCurrentIngredient();
    public int IngredientsCount => answerCardSO.IngredientsSO.Length;

    public DrawingMultiplayer Paint => paint;
    public IngredientManager IngredientManager => ingredientManager;
    public RoundManager RoundManager => roundManager;
    public RoleManager RoleManager => roleManager;
    public CardManager CardManager => cardManager;
    public GameObject SceneMonster => sceneMonster;
    public Animator SceneMonsterAnimator => sceneMonsterAnimator;
    public SoundList SoundList => soundList;

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
        StartCoroutine(TakePack()); // modify for second mode
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
                ingredientManager = new SecondModeIngredientManager(this, gameConfig, compareSystem);
                roundManager = new SecondModeRoundManager(this, gameConfig, compareSystem, ingredientManager);
            }
            else
            {
                if (IsTeamMode)
                {
                    Log("TEAM MODE");

                    Debug.Assert(compareSystem != null);
                    ingredientManager = new TeamIngredientManager(this, gameConfig, compareSystem);
                    roundManager = new TeamRoundManager(this, gameConfig, compareSystem, ingredientManager);
                }
                else
                {
                    Log("COMPETITIVE MODE");

                    ingredientManager = new CompetitiveIngredientManager(this, gameConfig, compareSystem);
                    roundManager = new CompetitiveRoundManager(this, gameConfig, compareSystem, ingredientManager);
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
    }

    private void OnClientConnected(ulong clientId)
    {
        if (isGameEnded)
            return;

        roundAmount++;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (isGameEnded)
            return;

        roundAmount--;

        if (PainterId == clientId)
            nextRoundButton.onClick?.Invoke();

        LogWarning(NetworkManager.ConnectedClientsIds.Count);
        if (NetworkManager.ConnectedClientsIds.Count <= 2)
            OnRoundEnded();
    }

    private IEnumerator TakePack()
    {
        yield return new WaitForSeconds(0.1f);
        bestiary.TakePack();
        bestiaryIngredients.TakePack();
    }

    private void SetNRBRoles()
    {
        nextRoundButton.onClick.RemoveAllListeners();

        nextRoundButton.onClick.AddListener(() =>
        {
            if (IsServer)
                OnRoundStartedClientRpc();
            else
                OnRoundStartedServerRpc();
        });
        nextRoundButton.onClick.AddListener(roleManager.ChangeRoles);
        nextRoundButton.GetComponentInChildren<TextMeshProUGUI>().text = nextRoundButtonText;
    }

    private void SetNRBLobbyWithInterstitialAd(bool isWatched)
    {
        SetNRBLobby();
        VK_Connect.Instance.OnInterstitialAdTryWatched -= SetNRBLobbyWithInterstitialAd;
    }

    private void SetNRBLobby()
    {
        nextRoundButton.gameObject.SetActive(true);
        nextRoundButton.onClick.RemoveAllListeners();

        nextRoundButton.onClick.AddListener(ReturnToLobby);
        nextRoundButton.GetComponentInChildren<TextMeshProUGUI>().text = returnToLobbyButtonText;
    }

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
        if (clientId == NetworkManager.LocalClientId)
            NotificationSystem.Instance.SendLocal("Вы отгадали монстра!");
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnRoundStartedServerRpc()
    {
        OnRoundStartedClientRpc();
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

        sceneMonsterMaterial.mainTexture = answerCardSO.MonsterTexture;
        sceneObjectsManager.OnRoundEnded();
        hintManager.DisableHandHint();
        TokenManager.AccrueTokens();

        BackgroundMusic.Instance.Play("tokens_take-card");

        answerCardSO = null;
    }

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

            Debug.LogWarning("Showing monster!");

            hintManager.SetHintData(answerCardSO.Id);
            hintManager.DisableHandHint();
        }
        else
        {
            sceneMonsterMaterial.mainTexture = hiddenMonsterTexture;
            SceneMonster.SetActive(true);
            SceneMonsterAnimator.Play("Idle");
        }

        playersStatusManager.ResetStatuses();
        BackgroundMusic.Instance.Play("monsterGuess");
        OnGuessMonsterStageActivatedOnClient?.Invoke(IsPainter);
    }

    private void ActivateGuessMonsterStage()
    {
        Stage = Stage.MonsterGuess;
        paint.ClearCanvasGlobal();

        bestiaryIngredients.gameObject.SetActive(false);

        ActivateGuessMonsterStageClientRpc();

        timer.SetMonsterGuessTime();
        timer.StartTimer();
    }

    [ClientRpc]
    private void OnMonsterGuessedClientRpc()
    {
        SceneMonster.SetActive(true);
        SceneMonsterAnimator.Play("Win");
    }

    [ClientRpc]
    private void OnMonsterNotGuessedClientRpc()
    {
        SceneMonster.SetActive(true);
        SceneMonsterAnimator.Play("Loose");
    }

    public void AllPlayersGuessed()
    {
        timer.PrematureStop();
    }

    #endregion
    #region SetCardSO

    [ClientRpc]
    private void SetCardSOClientRpc(byte cardSOIndex)
    {
        answerCardSO = cardManager.GetCardSOByIndex(cardSOIndex);

        sceneMonsterMaterial.mainTexture = answerCardSO.MonsterTexture;
        OnCardChoosedOnClient?.Invoke(answerCardSO);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCardSOServerRpc(byte cardSOIndex)
    {
        answerCardSO = cardManager.GetCardSOByIndex(cardSOIndex);
        sceneMonsterMaterial.mainTexture = answerCardSO.MonsterTexture;

        ingredientManager.SetIngredients(answerCardSO.IngredientsSO.Length);

        SetCardSOClientRpc(cardSOIndex);

        SetHintDataClientRpc((sbyte)ingredientManager.GetCurrentIngredientIndex);

        //Stage = Stage.IngredientGuess;

        Log("New answer CardSO: " + answerCardSO.Id);
    }

    private void SetAnswerCardSO(byte cardSOIndex)
    {
        answerCardSO = cardManager.GetCardSOByIndex(cardSOIndex);

        sceneMonsterMaterial.mainTexture = answerCardSO.MonsterTexture;
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
            hintManager.SetHintData(answerCardSO.Id);
        else
            hintManager.SetHintData(answerCardSO.IngredientsSO[ingredientIndex].Name);
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

    private void OnIngredientSwitched(sbyte ingredientIndex)
    {
        timer.SetIngredientGuessTime();
        timer.StartTimer();
        paint.ClearCanvasGlobal();

        OnIngredientSwitchedClientRpc(ingredientIndex);
    }

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
        
        StartCoroutine(TokenManager.OnGameEnded());

        if (!IsTeamMode)
        {
            ulong winnerClientId;
            int winnerClientTokens;
            (winnerClientId, winnerClientTokens) = TokenManager.GetClientIdWithMaxTokens();

            NotificationSystem.Instance.SendLocal("Выиграл " + PlayersDataManager.Instance.PlayerDatas[winnerClientId].Name + "со счетом " + winnerClientTokens);
        }

        TokenManager.ResetData();
        OnGameEnded?.Invoke();
    }

    private void ReturnToLobby()
    {
        RelayManager.Instance.ReturnToLobby();
    }

    [ServerRpc(RequireOwnership = false)]
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
    }

    public void StartGame()
    {
        Logger.Instance.Log(this, Stage);
        if (answerCardSO == null || Stage != Stage.Waiting)
            return;

        Stage = Stage.IngredientGuess;

        StartGameServerRpc();
    }

    private void Log(object message) => Logger.Instance.Log(this, message);
    private void LogWarning(object message) => Logger.Instance.LogWarning(this, message);
    private void LogError(object message) => Logger.Instance.LogError(this, message);

    #region Get

    public string GetCurrentIngredient()
    {
        return answerCardSO.IngredientsSO[ingredientManager.GetCurrentIngredientIndex].Name;
    }

    public string GetCurrentMonster()
    {
        return answerCardSO.Id;
    }

    #endregion
}