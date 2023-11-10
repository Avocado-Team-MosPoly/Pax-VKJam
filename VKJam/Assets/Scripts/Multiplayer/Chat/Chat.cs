using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Chat : NetworkBehaviour
{
    public IReadOnlyList<Message> History => history;
    // нужно заполнить
    public IReadOnlyDictionary<ulong, string> PlayersNames => playersNames;

    [HideInInspector] public UnityEvent MessageSended = new();
    [HideInInspector] public UnityEvent MessageReceived = new();

    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private Button sendMessageButton;

    private List<Message> history = new();
    public Dictionary<ulong, string> playersNames;

    public struct Message : INetworkSerializable
    {
        public byte senderId;
        public FixedString64Bytes text;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref senderId);
            serializer.SerializeValue(ref text);
        }

        public override bool Equals(object obj)
        {
            return obj is Message message &&
                   senderId == message.senderId &&
                   text.Equals(message.text);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(senderId, text);
        }

        public static bool operator==(Message a, Message b)
        {
            if (a.senderId == b.senderId)
                if (a.text == b.text)
                    return true;
            
            return false;
        }

        public static bool operator !=(Message a, Message b)
        {
            if (a.senderId == b.senderId)
                if (a.text == b.text)
                    return false;

            return true;
        }
    }

    private void Awake()
    {
        sendMessageButton.onClick.AddListener(SendMessage_);

        messageInputField.characterLimit = new Message().text.Capacity;
    }

    public void SendMessage_()
    {
        Message msg = new()
        {
            senderId = (byte)NetworkManager.Singleton.LocalClientId,
            text = new FixedString64Bytes(messageInputField.text)
        };

        if (IsServer)
        {
            SendMessageClientRpc(msg);
        }
        else
            SendMessageServerRpc(msg);

        history.Add(msg);
        MessageSended?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMessageServerRpc(Message message)
    {
        SendMessageClientRpc(message);
    }

    [ClientRpc]
    private void SendMessageClientRpc(Message message)
    {
        if (message.senderId != NetworkManager.Singleton.LocalClientId)
        {
            history.Add(message);
            MessageReceived?.Invoke();
        }
    }
}