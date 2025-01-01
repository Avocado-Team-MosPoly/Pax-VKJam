using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine.UI;

public class Chat : NetworkBehaviour
{
    public struct Message : INetworkSerializable
    {
        public byte senderId;
        public FixedString64Bytes text;

        public static int Capacity => FixedString64Bytes.UTF8MaxLengthInBytes;

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

        public static bool operator ==(Message a, Message b)
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

    public UnityEvent OnMessageSended { get; private set; } = new();
    public UnityEvent OnMessageReceived { get; private set; } = new();

    private List<TMP_InputField> inputFields;
    private List<Button> buttons;

    private List<Message> history = new();

    public IReadOnlyList<Message> History => history;

    public void Enable()
    {
        foreach (TMP_InputField inputField in inputFields)
            inputField.interactable = true;

        foreach (Button button in buttons)
            button.interactable = true;
    }

    public void Disable()
    {
        foreach (TMP_InputField inputField in inputFields)
            inputField.interactable = false;

        foreach (Button button in buttons)
            button.interactable = false;
    }

    public void Send(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        Message msg = new()
        {
            senderId = (byte)NetworkManager.Singleton.LocalClientId,
            text = new FixedString64Bytes(message)
        };

        if (IsServer)
            SendMessageClientRpc(msg);
        else
            SendMessageServerRpc(msg);

        history.Add(msg);
        OnMessageSended?.Invoke();
    }

    public void AddInputs(TMP_InputField inputField, Button button)
    {
        SetupInputs(inputField, button);

        inputFields.Add(inputField);
        buttons.Add(button);
    }

    private void SetupInputs(TMP_InputField inputField, Button button)
    {
        button.onClick.AddListener(() =>
        {
            Send(inputField.text);
            inputField.text = string.Empty;
        });

        inputField.onSubmit.AddListener(text =>
        {
            Send(text);
            inputField.text = string.Empty;
        });

        inputField.characterLimit = Message.Capacity;
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
            OnMessageReceived?.Invoke();
        }
    }
}