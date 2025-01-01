public class SecondThirdStageManager : BaseStageManager
{
    public override void StartStage()
    {
        SecondModeManager.Instance.MainGameRoot.SetActive(true);

        SecondModeManager.Instance.Stage = SecondModeStage.Waiting;

        GameManager.Instance.OnIngredientGuessStartedOnClient.AddListener(OnIngredientGuessStarted);
        GameManager.Instance.OnGuessMonsterStageActivatedOnClient.AddListener(OnMonsterGuessStarted);
        GameManager.Instance.OnGameEnded.AddListener(FinishStage);

        SecondModeManager.Instance.ReadinessSystem.DisableVisual();

        base.StartStage();
    }

    public override void FinishStage()
    {
        SecondModeManager.Instance.MainGameRoot.SetActive(false);

        GameManager.Instance.OnIngredientGuessStartedOnClient.RemoveListener(OnIngredientGuessStarted);
        GameManager.Instance.OnGuessMonsterStageActivatedOnClient.RemoveListener(OnMonsterGuessStarted);
        GameManager.Instance.OnGameEnded.RemoveListener(FinishStage);

        SecondModeManager.Instance.ReadinessSystem.EnableVisual();

        base.FinishStage();
    }

    private void OnIngredientGuessStarted()
    {
        SecondModeManager.Instance.Stage = SecondModeStage.IngredientGuess;
    }

    private void OnMonsterGuessStarted(bool isPainter)
    {
        SecondModeManager.Instance.Stage = SecondModeStage.MonsterGuess;
    }
}