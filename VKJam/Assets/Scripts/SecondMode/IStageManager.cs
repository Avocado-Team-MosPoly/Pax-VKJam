using System;
using UnityEngine;

public interface IStageManager
{
    public event Action Started;
    public event Action Finished;

    public void StartStage();

    public void FinishStage();
}

public abstract class BaseStageManager : MonoBehaviour, IStageManager
{
    public event Action Started;
    public event Action Finished;

    public virtual void StartStage()
    {
        Started?.Invoke();
    }

    public virtual void FinishStage()
    {
        Finished?.Invoke();
    }
}