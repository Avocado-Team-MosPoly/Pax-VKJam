using System;
using System.Collections.Generic;
using UnityEngine;

public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance { get; private set; }

    [SerializeField] private Notification notificationPrefab;

    //[SerializeField] private float showTime = 3.0f;

    private Notification notificationInstance;

    private List<string> notificationQueue = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[MessageSystem] Two or more Message Systems on scene");
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);

            notificationInstance = Instantiate(notificationPrefab, transform);
            notificationInstance.gameObject.SetActive(false);
            notificationInstance.OnDisappeared += OnNotificationDisappear;
        }
    }

    private void OnNotificationDisappear()
    {
        if (notificationQueue.Count <= 0)
            return;

        Send(notificationQueue[0]);
        notificationQueue.RemoveAt(0);
    }
    public void sss()
    {
        Send("New new new");
    }
    public void Send(object message)
    {
        if (message == null)
            throw new NullReferenceException($"{nameof(message)} is null");
        if (notificationInstance == null)
            throw new NullReferenceException($"{nameof(notificationInstance)} is null. Don't use \"{nameof(Send)}\" method on Awake");

        if (notificationInstance.IsActive)
        {
            notificationQueue.Add(message.ToString());
        }
        else
        {
            notificationInstance.SetData(message.ToString());
            notificationInstance.Send();
        }
    }
}