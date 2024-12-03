using System;
using UnityEngine;

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