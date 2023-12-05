using System;
using System.ComponentModel;
using System.Reflection;
using TMPro;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public static Logger Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI output;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private string GetClassName(object obj)
    {
        //return string.Empty;
        return obj is Type ? (obj as Type).Name : obj.GetType().Name;
    }

    public void Log(object sender, object message)
    {
        string senderClassName = GetClassName(sender);

        Debug.Log($"[{senderClassName}] {message}");
        //NotificationSystem.Instance.SendLocal(message);
        //output.text += message + "\n";
    }

    public void LogWarning(object sender, object message)
    {
        string senderClassName = GetClassName(sender);

        Debug.LogWarning($"[{senderClassName}] {message}");
        //NotificationSystem.Instance.SendLocal(message);
        //output.text += message + "\n";
    }

    public void LogError(object sender, object message)
    {
        string senderClassName = GetClassName(sender);

        Debug.LogError($"[{senderClassName}] {message}");
        //NotificationSystem.Instance.SendLocal(message);
        //output.text += message + "\n";
    }
}