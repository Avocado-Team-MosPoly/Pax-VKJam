using UnityEngine;

public abstract class BaseSingleton<T> : MonoBehaviour where T : BaseSingleton<T>
{
    public static T Instance { get; protected set; }

    protected virtual void Awake()
    {
        if (Instance == null)
            Instance = (T)this;
        else
            Logger.Instance.LogWarning(this, $"Created two or more objects of type: {this.GetType().Name}");
    }
}