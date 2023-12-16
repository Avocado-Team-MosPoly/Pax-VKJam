using UnityEngine;

public abstract class TaskExecutor<T> : MonoBehaviour where T : TaskExecutor<T>
{
    public static T Executor { get; protected set; }

    private void Awake()
    {
        if (Executor == null)
            Executor = (T)this;
        else
            Logger.Instance.LogWarning(this, $"Two or more objects of type: {this.GetType().Name}");
    }
}