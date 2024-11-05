using System;

public interface IStageManager
{
    public event Action Started;
    public event Action Finished;

    public void StartStage();

    public void FinishStage();
}