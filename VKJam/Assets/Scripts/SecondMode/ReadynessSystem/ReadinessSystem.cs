using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ReadinessSystem : NetworkBehaviour
{
    public event Action AllReady;
    public event Action<ulong> Ready;

    [SerializeField] private Toggle toggle;
    [SerializeField] private TextMeshProUGUI readyClientsCountLabel;

    private List<ClientReadiness> clientsReadiness;
    
    public int ReadyClientsCount { get; private set; }

    private void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    public override void OnNetworkSpawn()
    {
        clientsReadiness = new(PlayersDataManager.Instance.PlayerDatas.Count);
        IEnumerable<ulong> connectedClients = PlayersDataManager.Instance.PlayerDatas.Keys;

        foreach (ulong clientId in connectedClients)
            clientsReadiness.Add(new ClientReadiness(clientId, false));

        PlayersDataManager.Instance.PlayerConnected += OnClientConnected;
        PlayersDataManager.Instance.PlayerDisconnected += OnClientDisconnected;
    }

    public override void OnNetworkDespawn()
    {
        PlayersDataManager.Instance.PlayerConnected += OnClientConnected;
        PlayersDataManager.Instance.PlayerDisconnected += OnClientDisconnected;
    }

    public void SetAllUnready()
    {
        SetAllUnreadyServerRpc();
    }

    [ServerRpc]
    private void SetAllUnreadyServerRpc()
    {
        SetAllUnreadyClientRpc();
    }

    [ClientRpc]
    private void SetAllUnreadyClientRpc()
    {
        foreach (ClientReadiness client in clientsReadiness)
            client.isReady = false;
    }

    [ServerRpc]
    private void SendReadyServerRpc(bool value, ServerRpcParams rpcParams)
    {
        SetReady(rpcParams.Receive.SenderClientId, value);
        SendReadyClientRpc(rpcParams.Receive.SenderClientId, value);
    }

    [ClientRpc]
    private void SendReadyClientRpc(ulong clientId, bool value)
    {
        if (IsServer || clientId == NetworkManager.LocalClientId)
            return;

        SetReady(clientId, value);
    }

    private void SetReady(ulong clientId, bool value)
    {
        for (int i = 0; i < clientsReadiness.Count; i++)
        {
            if (clientsReadiness[i].id == clientId)
            {
                if (clientsReadiness[i].isReady != value)
                {
                    clientsReadiness[i].isReady = value;

                    if (value)
                        ReadyClientsCount++;
                    else
                        ReadyClientsCount--;

                    UpdateView();

                    return;
                }

                break;
            }
        }

        Debug.LogError($"[{nameof(ReadinessSystem)}] Client ({clientId}) not found");
    }

    private void UpdateView()
    {
        readyClientsCountLabel.text = $"{ReadyClientsCount} / {clientsReadiness.Count}";
    }

    private void OnClientConnected(ulong id)
    {
        clientsReadiness.Add(new ClientReadiness(id, false));
    }

    private void OnClientDisconnected(ulong id)
    {
        for (int i = 0; i < clientsReadiness.Count; i++)
        {
            if (clientsReadiness[i].id == id)
            {
                clientsReadiness.RemoveAt(i);
                break;
            }
        }
    }

    private void OnToggleValueChanged(bool value)
    {
        StartCoroutine(ToggleClickCooldown());

        SetReady(NetworkManager.LocalClientId, value);
        SendReadyServerRpc(value, new ServerRpcParams());
    }

    private IEnumerator ToggleClickCooldown()
    {
        toggle.interactable = false;

        yield return new WaitForSeconds(1f);

        toggle.interactable = true;
    }

    private class ClientReadiness
    {
        public readonly ulong id;
        public bool isReady;

        public ClientReadiness(ulong clientId, bool isReady)
        {
            this.id = clientId;
            this.isReady = isReady;
        }
    }
}