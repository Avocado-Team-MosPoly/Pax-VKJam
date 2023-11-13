using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    /// <summary> Sends true if local player is painter, false if not </summary>
    [HideInInspector] public UnityEvent<bool> OnGuessMonsterStageActivatedOnClient;
    [HideInInspector] public UnityEvent OnAnswerCardSOSettedOnClient; // TODO: remind why or delete
    [HideInInspector] public UnityEvent OnGameEnded;

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

    [Header("Managers")]
    [SerializeField] private RoleManager roleManager;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private CompareSystem compareSystem;
    [SerializeField] private SceneObjectsManager sceneObjectsManager;
    [SerializeField] private HintManager hintManager;

    private IngredientManager ingredientManager;
    private RoundManager roundManager;

    [Header("---")]
    [SerializeField] private Paint paint;
    [SerializeField] private GameObject nextRoundButton;
    [SerializeField] private GameObject returnToLobbyButton;

    [Header("Scene Monster")]
    [SerializeField] private GameObject sceneMonster;
    [SerializeField] private Material sceneMonsterMaterial;
    [SerializeField] private Texture hiddenMonster;

    private int currentRound = 1;
    private int roundAmount = 4;

    private CardSO answerCardSO;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            LogWarning($"Two GameManagers on scene:\n{Instance}, {this}");

        sceneMonsterMaterial = sceneMonster.GetComponent<Renderer>().material;
    }

    private void Start()
    {
        cardManager.OnChooseCard.AddListener(SetAnswerCardSO);

        nextRoundButton.GetComponent<Button>().onClick.AddListener(roleManager.ChangeRoles);
    }

    public override void OnNetworkSpawn()
    {
        Log("IsServer : " + IsServer);

        if (IsServer)
        {
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
            
            Timer.Instance.OnExpired.AddListener(OnTimeExpired);
            
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
            Timer.Instance.OnIngredientGuess();
            roundManager.OnTimeExpired();
        }
        else
            ingredientManager.OnTimeExpired();
    }

    [ClientRpc]
    private void OnRoundEndedClientRpc()
    {
        Stage = Stage.Waiting;

        sceneMonsterMaterial.mainTexture = answerCardSO.MonsterTexture;
        sceneObjectsManager.OnRoundEnded();
        hintManager.DisableHandHint();
        TokenManager.AccrueTokens();
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
            sceneMonsterMaterial.mainTexture = hiddenMonster;
        }

        OnGuessMonsterStageActivatedOnClient?.Invoke(IsPainter);
    }

    private void ActivateGuessMonsterStage()
    {
        Stage = Stage.MonsterGuess;
        paint.ClearCanvas();

        ActivateGuessMonsterStageClientRpc();

        Timer.Instance.OnMonsterGuess();
        Timer.Instance.StartServerRpc();

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
        
        OnAnswerCardSOSettedOnClient?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCardSOServerRpc(byte cardSOIndex)
    {
        answerCardSO = cardManager.GetCardSOByIndex(cardSOIndex);
        sceneMonsterMaterial.mainTexture = answerCardSO.MonsterTexture;

        SetCardSOClientRpc(cardSOIndex);

        SetHintDataClientRpc((sbyte)ingredientManager.CurrentIngredientIndex);
        Timer.Instance.OnIngredientGuess();
        Timer.Instance.StartServerRpc();

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

    [ClientRpc]
    private void SetHintDataClientRpc(sbyte ingredientIndex)
    {
        if (!IsPainter)
            return;

        if (ingredientIndex < 0)
            hintManager.SetHintData(answerCardSO.Id);
        else
            hintManager.SetHintData(answerCardSO.Ingredients[ingredientIndex]);
    }

    private void OnIngredientSwitched(sbyte ingredientIndex)
    {
        Timer.Instance.OnIngredientGuess();
        Timer.Instance.StartServerRpc();
        paint.ClearCanvas();

        SetHintDataClientRpc(ingredientIndex);
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

    private void Log(object message) => Debug.Log($"[{name}] {message}");
    private void LogWarning(object message) => Debug.LogWarning($"[{name}] {message}");

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