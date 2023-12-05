using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NotificationSystem : NetworkBehaviour
{
    public static NotificationSystem Instance { get; private set; }

    [SerializeField] private Notification notificationPrefab;

    private Notification notificationInstance;
    private List<NotificationQueueItem> notificationQueue = new();

    private class NotificationQueueItem
    {
        public string Text { get; private set; }
        public float ShowTime { get; private set; }

        public NotificationQueueItem(string text, float showTime = 3)
        {
            Text = text;
            ShowTime = showTime;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning($"[{this.name}] Two or more Message Systems on scene");
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    private void SpawnNotificationPrefab()
    {
        if (notificationInstance != null)
        {
            Logger.Instance.LogWarning(this, $"{nameof(notificationInstance)} is already declared");
            return;
        }

        notificationInstance = Instantiate(notificationPrefab, transform).Init();
        notificationInstance.gameObject.SetActive(false);
        notificationInstance.OnDisappeared += OnNotificationDisappear;
    }

    private void OnNotificationDisappear()
    {
        if (notificationQueue.Count <= 0)
            return;

        SendLocal(notificationQueue[0]);
        notificationQueue.RemoveAt(0);
    }

    #region Send
    private void SendLocal(NotificationQueueItem notification)
    {
        SendLocal(notification.Text, notification.ShowTime);
    }

    #region Network

    [ServerRpc(RequireOwnership = false)]
    private void SendGlobalServerRpc(string message, float showTime, ServerRpcParams serverRpcParams)
    {
        if (serverRpcParams.Receive.SenderClientId != NetworkManager.LocalClientId)
            SendLocal(message, showTime);

        SendGlobalClientRpc(message, showTime, (byte)serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void SendGlobalClientRpc(string message, float showTime, byte senderClientId)
    {
        if (IsServer)
            return;
        if (senderClientId == NetworkManager.LocalClientId)
            return;

        SendLocal(message, showTime);
    }

    #endregion

    public void SendGlobal(object message, float showTime = 3f)
    {
        SendLocal(message, showTime);

        if (CustomNetworkManager.Singleton == null || !CustomNetworkManager.Singleton.IsClient)
        {
            Logger.Instance.LogError(this, $"You should connect to relay and start host/client before call {nameof(SendGlobal)}");
            return;
        }

        string stringMessage = message.ToString();
        ServerRpcParams serverRpcParams = new();

        if (IsServer)
            SendGlobalClientRpc(stringMessage, showTime, (byte)serverRpcParams.Receive.SenderClientId);
        else
            SendGlobalServerRpc(stringMessage, showTime, serverRpcParams);
    }

    public void SendLocal(object message, float showTime = 3f)
    {
        if (message == null)
            Logger.Instance.LogError(this, new NullReferenceException($"{nameof(message)} is null"));
        if (notificationInstance == null)
        {
            SpawnNotificationPrefab();
            //Logger.Instance.LogError(this, new NullReferenceException($"{nameof(notificationInstance)} is null. Don't use \"{nameof(SendLocal)}\" method on Awake"));
        }

        if (notificationInstance.IsActive)
        {
            notificationQueue.Add( new(message.ToString(), showTime) );
        }
        else
        {
            notificationInstance.SetData(message.ToString(), showTime);
            notificationInstance.Send();
        }
    }

    #endregion
}