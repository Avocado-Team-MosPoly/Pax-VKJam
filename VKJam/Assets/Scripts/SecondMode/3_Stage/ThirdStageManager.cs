using UnityEngine;

public class ThirdStageManager : BaseStageManager
{
    [SerializeField] private NetworkCountdownTimer timer;

    public override void StartStage()
    {
        // start timer
        // guess monster

        // readiness system

        base.StartStage();
    }

    public override void FinishStage()
    {
        // check guesses

        base.FinishStage();
    }
}