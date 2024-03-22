using System;
using TMPro;
using UnityEngine;

public class Logger : BaseSingleton<Logger>
{
    [SerializeField] private TextMeshProUGUI output;

    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
            DontDestroyOnLoad(gameObject);
        else
            Destroy(gameObject);
    }

    private string GetClassName(object obj)
    {
        return obj is Type ? (obj as Type).Name : obj.GetType().Name;
    }

    public void Log(object sender, object message)
    {
        string senderClassName = GetClassName(sender);

        Debug.Log($"[{senderClassName}] {message ?? "Null"}");
        //NotificationSystem.Instance.SendLocal(message);
        //output.text += message + "\n";
    }

    public void LogWarning(object sender, object message)
    {
        string senderClassName = GetClassName(sender);

        Debug.LogWarning($"[{senderClassName}] {message ?? "Null"}");
        //NotificationSystem.Instance.SendLocal(message);
        //output.text += message + "\n";
    }

    public void LogError(object sender, object message)
    {
        string senderClassName = GetClassName(sender);

        Debug.LogError($"[{senderClassName}] {message ?? "Null"}");
        //NotificationSystem.Instance.SendLocal(message);
        //output.text += message + "\n";
    }
}