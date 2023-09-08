using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    [Header("Managers")]
    [SerializeField] private RoleManager roleManager;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private CompareSystem compareSystem;
    [SerializeField] private IngredientManager ingredientManager;
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private SceneObjectsManager sceneObjectsManager;

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

    public CardSO AnswerCardSO { get; private set; }
    public Stage Stage { get; private set; } = Stage.Waiting;

    #region GetProperties
    
    public Paint Paint => paint;
    public CardManager CardManager => cardManager;
    public RoleManager RoleManager => roleManager;
    public GameObject SceneMonster => sceneMonster;
    public bool IsPainter => roleManager.PainterId == NetworkManager.Singleton.LocalClientId;

    #endregion

    /// <summary> Sends true if local player is painter, false if not </summary>
    [HideInInspector] public UnityEvent<bool> OnGuessMonsterStageActivated;

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
        compareSystem.OnGuess.AddListener(ingredientManager.CompareIngredient);
        ingredientManager.OnIngredientsEnded.AddListener(ActivateGuessMonsterStage);
    }

    public override void OnNetworkSpawn()
    {
        Log($"IsServer : {IsServer}");

        if (IsServer)
        {
            nextRoundButton.gameObject.SetActive(true);
            returnToLobbyButton.gameObject.SetActive(true);

            Timer.Instance.OnExpired.AddListener(OnTimeExpired);

            roundAmount = int.Parse(LobbyManager.Instance.CurrentLobby.Data[LobbyManager.Instance.KEY_ROUND_AMOUNT].Value);
        }
        else
        {
            nextRoundButton.gameObject.SetActive(false);
            returnToLobbyButton.gameObject.SetActive(false);
        }
    }

    private void OnTimeExpired()
    {
        if (Stage == Stage.MonsterGuess)
            roundManager.OnTimeExpired();
        else
            ingredientManager.OnTimeExpired();
    }

    [ClientRpc]
    private void ActivateGuessMonsterStageClientRpc()
    {
        if (IsPainter)
        {
            paint.SetActive(false);
        }
        else
        {
            sceneMonster.SetActive(true);
            sceneMonsterMaterial.mainTexture = hiddenMonster;
        }

        OnGuessMonsterStageActivated?.Invoke(IsPainter);
    }

    private void ActivateGuessMonsterStage()
    {
        ActivateGuessMonsterStageClientRpc();
    }

    #region SetCardSO

    [ClientRpc]
    private void SetMonsterTextureClientRpc(byte cardSOIndex)
    {
        CardSO cardSO = cardManager.GetCardSOByIndex(cardSOIndex);

        sceneMonsterMaterial.mainTexture = cardSO.MonsterTexture;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCardSOServerRpc(byte cardSOIndex)
    {
        AnswerCardSO = cardManager.GetCardSOByIndex(cardSOIndex);

        //currentIngredientIndex = 0;
        //SetHintDataClientRpc((sbyte)currentIngredientIndex);
        //SetMonsterTextureClientRpc(cardSOIndex);
        sceneMonsterMaterial.mainTexture = AnswerCardSO.MonsterTexture;

        Timer.Instance.StartServerRpc();

        Log("New answer CardSO: " + AnswerCardSO.Id);
    }

    private void SetAnswerCardSO(byte cardSOIndex)
    {
        AnswerCardSO = cardManager.GetCardSOByIndex(cardSOIndex);

        sceneMonsterMaterial.mainTexture = AnswerCardSO.MonsterTexture;
        sceneMonster.SetActive(true);

        SetCardSOServerRpc(cardSOIndex);
    }

    #endregion
    #region Get

    public string GetCurrentIngredient()
    {
        return AnswerCardSO.Ingredients[ingredientManager.CurrentIngredientIndex];
    }

    public string GetCurrentMonster()
    {
        return AnswerCardSO.Id;
    }

    #endregion

    private void Log(object message) => Debug.Log($"[{name}] {message}");
    private void LogWarning(object message) => Debug.LogWarning($"[{name}] {message}");
}