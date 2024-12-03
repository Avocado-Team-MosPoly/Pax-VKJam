using Unity.Netcode;
using UnityEngine;

public class SecondModeManager : BaseSingleton<SecondModeManager>, IGameManager
{
    [Header("Systems")]
    [SerializeField] private SecondModeGuessSystem guessSystem;
    [SerializeField] private ReadinessSystem readinessSystem;
    [SerializeField] private BaseStageManager[] stageManagers;
    [SerializeField] private TokenManager tokenManager;

    // [SerializeField] private <note manager>

    [Header("Config")]
    [SerializeField] private GameConfigSO gameConfig;

    private IngredientManager ingredientManager;
    private RoundManager roundManager;

    private int currentStageIndex = -1;

    #region Properties

    public SecondModeStage Stage { get; private set; }
    public SecondModeGuessSystem GuessSystem => guessSystem;
    public ReadinessSystem ReadinessSystem => readinessSystem;

    public bool IsTeamMode => throw new System.NotImplementedException();
    public int CurrentRound => throw new System.NotImplementedException();

    public bool IsDangerousCard => throw new System.NotImplementedException();
    public string CurrentMonsterName => throw new System.NotImplementedException();

    public string CurrentIngredientName => throw new System.NotImplementedException();
    public int IngredientsCount => throw new System.NotImplementedException();

    public bool IsPainter => throw new System.NotImplementedException();
    public byte PainterId => throw new System.NotImplementedException();

    #endregion

    private void Start()
    {
        NetworkManager.Singleton.OnClientStarted += Init;
    }

    private void Init()
    {
        //ingredientManager = new CompetitiveIngredientManager(this, gameConfig, guessSystem);
        //roundManager = new CompetitiveRoundManager(this, gameConfig, guessSystem, ingredientManager);

        stageManagers[0].StartStage();
        currentStageIndex = 0;
    }

    public void SetStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= stageManagers.Length - 1)
            return;

        stageManagers[currentStageIndex].FinishStage();
        stageManagers[stageIndex].StartStage();

        currentStageIndex = stageIndex;
    }

    public void NextStage()
    {
        SetStage(currentStageIndex + 1);
    }

    public void PreviousStage()
    {
        SetStage(currentStageIndex - 1);
    }

    public void ReturnToLobby()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError($"[{nameof(SecondModeManager)}] Return to Lobby available only on server");
            return;
        }

        RelayManager.Instance.ReturnToLobby();
    }
}