using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// TODO: return real method realization after local tests

public class ReadinessSystem : NetworkBehaviour
{
    public event Action OnAllReady;
    public event Action<bool> OnLocalReadyChanged;

    [SerializeField] private GameObject visual;
    [SerializeField] private Toggle toggle;
    [SerializeField] private TextMeshProUGUI readyClientsCountLabel;

    private List<ClientReadiness> clientsReadiness;
    
    public bool LocalReadiness { get; private set; }
    public int ReadyClientsCount { get; private set; }
    public bool IsVisualEnabled => visual.activeSelf;

    private void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    public override void OnNetworkSpawn()
    {
        clientsReadiness = new List<ClientReadiness>(PlayersDataManager.Instance.PlayersData.Count);
        IEnumerable<ulong> connectedClients = PlayersDataManager.Instance.PlayersData.Keys;

        foreach (ulong clientId in connectedClients)
            clientsReadiness.Add(new ClientReadiness(clientId, false));

        UpdateView();

        PlayersDataManager.Instance.PlayerConnected += OnClientConnected;
        PlayersDataManager.Instance.PlayerDisconnected += OnClientDisconnected;
    }

    public override void OnNetworkDespawn()
    {
        return;
        PlayersDataManager.Instance.PlayerConnected -= OnClientConnected;
        PlayersDataManager.Instance.PlayerDisconnected -= OnClientDisconnected;
    }

    public void SetAllUnready()
    {
        SetAllUnreadyServerRpc();
    }

    public void EnableVisual()
    {
        visual.SetActive(true);
    }

    public void DisableVisual()
    {
        visual.SetActive(false);
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
        {
            client.IsReady = false;

            if (client.Id == NetworkManager.LocalClientId)
                OnLocalReadyChanged?.Invoke(false);
        }

        LocalReadiness = false;
        ReadyClientsCount = 0;
        UpdateView();
    }

    [ServerRpc]
    private void SendReadyServerRpc(bool value, ServerRpcParams rpcParams)
    {
        if (rpcParams.Receive.SenderClientId != NetworkManager.LocalClientId)
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
            if (clientsReadiness[i].Id == clientId)
            {
                if (clientsReadiness[i].IsReady != value)
                {
                    clientsReadiness[i].IsReady = value;

                    if (value)
                        ReadyClientsCount++;
                    else
                        ReadyClientsCount--;

                    if (clientId == NetworkManager.LocalClientId)
                    {
                        LocalReadiness = value;
                        OnLocalReadyChanged?.Invoke(value);
                    }

                    UpdateView();
                    CheckAllReady();

                    return;
                }

                break;
            }
        }

        Debug.LogError($"[{nameof(ReadinessSystem)}] Client ({clientId}) has same ready flag or not found");
    }

    private void UpdateView()
    {
        toggle.SetIsOnWithoutNotify(LocalReadiness);
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
            if (clientsReadiness[i].Id == id)
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

    private void CheckAllReady()
    {
        if (ReadyClientsCount >= clientsReadiness.Count)
            OnAllReady?.Invoke();
    }
}