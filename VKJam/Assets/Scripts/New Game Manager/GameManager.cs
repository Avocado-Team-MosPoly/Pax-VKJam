using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary> Sends true if local player is painter, false if not </summary>
    [HideInInspector] public UnityEvent<bool> OnGuessMonsterStageActivatedOnClient;
    [HideInInspector] public UnityEvent OnGameEnded;
    [HideInInspector] public UnityEvent OnIngredientSwitchedOnClient;
    [HideInInspector] public UnityEvent OnRoundStartedOnClient;

    #region Properties

    public Stage Stage { get; private set; } = Stage.Waiting;
    public bool IsTeamMode { get; private set; }

    public int IngredientsCount => answerCardSO.Ingredients.Length;
    public bool IsDangerousCard => answerCardSO.Difficulty == CardDifficulty.Dangerous;
    public int CurrentRound => currentRound;

    public Paint Paint => paint;
    public IngredientManager IngredientManager => ingredientManager;
    public RoundManager RoundManager => roundManager;
    public RoleManager RoleManager => roleManager;
    public CardManager CardManager => cardManager;
    public GameObject SceneMonster => sceneMonster;
    public bool IsPainter => roleManager.PainterId == NetworkManager.Singleton.LocalClientId;
    public byte PainterId => roleManager.PainterId;

    #endregion

    [SerializeField] private GameConfigSO gameConfig;
    [SerializeField] private Timer timer;

    [Header("Managers")]
    [SerializeField] private RoleManager roleManager;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private CompareSystem compareSystem;
    [SerializeField] private PlayersStatusManager playersStatusManager;
    [SerializeField] private SceneObjectsManager sceneObjectsManager;
    [SerializeField] private HintManager hintManager;

    [Header("Bestiary")]
    [SerializeField] private Bestiary bestiary;
    [SerializeField] private BestiaryIngredients bestiaryIngredients;

    [Header("Paint")]
    [SerializeField] private Paint paint;
    
    [Header("Buttons")]
    [SerializeField] private GameObject nextRoundButton;
    [SerializeField] private GameObject returnToLobbyButton;

    [Header("Scene Monster")]
    [SerializeField] private GameObject sceneMonster;
    [SerializeField] private Material sceneMonsterMaterial;
    [SerializeField] private Texture hiddenMonsterTexture;

    private IngredientManager ingredientManager;
    private RoundManager roundManager;

    private int currentRound = 1;
    private int roundAmount = 4;

    private CardSO answerCardSO;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            LogError($"Two GameManagers on scene:\n{Instance}, {this}");

        sceneMonsterMaterial = sceneMonster.GetComponent<Renderer>().material;
    }

    private void Start()
    {
        cardManager.OnChooseCard.AddListener(SetAnswerCardSO);

        Button nrbb = nextRoundButton.GetComponent<Button>();
        nrbb.onClick.AddListener(() =>
        {
            if (IsServer)
                OnRoundStartedClientRpc();
            else
                OnRoundStartedServerRpc();
        });
        nrbb.onClick.AddListener(roleManager.ChangeRoles);
    }

    public override void OnNetworkSpawn()
    {
        Log("IsServer : " + IsServer);

        bestiary.TakePack();
        bestiaryIngredients.TakePack();

        timer.Init(gameConfig);

        if (IsServer)
        {
            timer.OnExpired.AddListener(OnTimeExpired);

            IsTeamMode = LobbyManager.Instance.CurrentLobby.Data[LobbyManager.Instance.KEY_TEAM_MODE].Value == "True";

            if (IsTeamMode)
            {
                Log("TEAM MODE");

                ingredientManager = new TeamIngredientManager(gameConfig, compareSystem);
                roundManager = new TeamRoundManager(gameConfig, compareSystem, ingredientManager);
            }
            else
            {
                Log("COMPETITIVE MODE");

                ingredientManager = new CompetitiveIngredientManager(gameConfig, compareSystem);
                roundManager = new CompetitiveRoundManager(gameConfig, compareSystem, ingredientManager);
            }

            ingredientManager.OnIngredientsEnded.AddListener(ActivateGuessMonsterStage);
            ingredientManager.OnIngredientSwitched.AddListener(OnIngredientSwitched);
            roundManager.OnRoundEnded.AddListener(OnRoundEnded);

            roundAmount = NetworkManager.Singleton.ConnectedClientsIds.Count;

            nextRoundButton.SetActive(true);
            returnToLobbyButton.SetActive(true);
        }
        else
        {
            nextRoundButton.SetActive(false);
            returnToLobbyButton.SetActive(false);
        }
    }

    private void OnTimeExpired()
    {
        if (Stage == Stage.MonsterGuess)
        {
            timer.OnIngredientGuess();
            roundManager.OnTimeExpired();
        }
        else
            ingredientManager.OnTimeExpired();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnRoundStartedServerRpc()
    {
        OnRoundStartedClientRpc();
    }

    [ClientRpc]
    private void OnRoundStartedClientRpc()
    {
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

        if (IsPainter)
        {
            playersStatusManager.OnRoundEnded();
            playersStatusManager.SetActive(true);
        }
        else if (IsTeamMode)
            playersStatusManager.SetActive(true);
        else
            playersStatusManager.SetActive(false);
    }

    private void OnRoundEnded()
    {
        currentRound++;

        if (currentRound > roundAmount)
            EndGame();

        Stage = Stage.Waiting;

        OnRoundEndedClientRpc();
    }

    #region GuessMonsterStage

    [ClientRpc]
    private void ActivateGuessMonsterStageClientRpc()
    {
        Stage = Stage.MonsterGuess;
        
        if (IsPainter)
        {
            paint.SetActive(false);
            if (paint.enabled)
                paint.GetComponent<Animator>().Play("NoteBookClose");

            hintManager.SetHintData(answerCardSO.Id);
            hintManager.DisableHandHint();
        }
        else
        {
            sceneMonster.SetActive(true);
            sceneMonsterMaterial.mainTexture = hiddenMonsterTexture;
        }

        OnGuessMonsterStageActivatedOnClient?.Invoke(IsPainter);
    }

    private void ActivateGuessMonsterStage()
    {
        Stage = Stage.MonsterGuess;
        paint.ClearCanvas();

        ActivateGuessMonsterStageClientRpc();

        timer.OnMonsterGuess();
        timer.StartServerRpc();

        Debug.Log("Monster Stage " + Stage);
    }

    #endregion
    #region SetCardSO

    [ClientRpc]
    private void SetCardSOClientRpc(byte cardSOIndex)
    {
        Stage = Stage.IngredientGuess;

        answerCardSO = cardManager.GetCardSOByIndex(cardSOIndex);

        sceneMonsterMaterial.mainTexture = answerCardSO.MonsterTexture;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCardSOServerRpc(byte cardSOIndex)
    {
        answerCardSO = cardManager.GetCardSOByIndex(cardSOIndex);
        sceneMonsterMaterial.mainTexture = answerCardSO.MonsterTexture;

        SetCardSOClientRpc(cardSOIndex);

        SetHintDataClientRpc((sbyte)ingredientManager.CurrentIngredientIndex);
        timer.OnIngredientGuess();
        timer.StartServerRpc();

        Stage = Stage.IngredientGuess;

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

    private void SetHintData(sbyte ingredientIndex)
    {
        if (!IsPainter)
            return;

        if (ingredientIndex < 0)
            hintManager.SetHintData(answerCardSO.Id);
        else
            hintManager.SetHintData(answerCardSO.Ingredients[ingredientIndex]);
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
        OnIngredientSwitchedOnClient?.Invoke();
    }

    private void OnIngredientSwitched(sbyte ingredientIndex)
    {
        timer.OnIngredientGuess();
        timer.StartServerRpc();
        paint.ClearCanvas();

        OnIngredientSwitchedClientRpc(ingredientIndex);
    }

    private void EndGame()
    {
        if (!IsTeamMode)
            TokenManager.OnCompetitiveGameEnd();

        EndGameClientRpc();

        nextRoundButton.GetComponent<Button>().onClick.RemoveAllListeners();
        nextRoundButton.GetComponent<Button>().onClick.AddListener(ReturnToLobby);
        nextRoundButton.GetComponentInChildren<TextMeshProUGUI>().text = "To Lobby";
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        OnGameEnded?.Invoke();
    }

    private void ReturnToLobby()
    {
        RelayManager.Instance.ReturnToLobby();
    }

    private void Log(object message) => Logger.Instance.Log($"[{name}] {message}");
    private void LogWarning(object message) => Debug.LogWarning($"[{name}] {message}");
    private void LogError(object message) => Debug.LogError($"[{name}] {message}");

    #region Get

    public string GetCurrentIngredient()
    {
        return answerCardSO.Ingredients[ingredientManager.CurrentIngredientIndex];
    }

    public string GetCurrentMonster()
    {
        return answerCardSO.Id;
    }

    #endregion
}