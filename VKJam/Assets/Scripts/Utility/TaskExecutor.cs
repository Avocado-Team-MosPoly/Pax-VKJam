using UnityEngine;

public abstract class TaskExecutor<T> : MonoBehaviour where T : TaskExecutor<T>
{
    private static T _instance; // back-up
    public static T _executor { get { return _instance; } protected set { _instance = value; } }
    [SerializeField] protected T Executor;
    private void Awake()
    {
        if (_executor == null)
        {
            _executor = Executor;
        }
    }
    protected void Denote()
    {
            _executor = (T)this;
    }
}