using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManagerUI : NetworkBehaviour
{
    //[SerializeField] private Button listPlayersButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button updatePlayerListButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private List<GameObject> playerGameObjectList = new();
    [SerializeField] private List<GameObject> playerReady = new();
    [SerializeField] private LobbyPlayerDataViewManager playerDataViewManager;

    [SerializeField] private string notReadyText;
    [SerializeField] private string readyText;

    //[SerializeField] private RectTransform playerListContainer;
    //[SerializeField] private GameObject playerInfoPrefab;
    [SerializeField] private NetworkList<bool> allPlayerReady = new();

    private NetworkList<byte> playersId = new();
    private TextMeshProUGUI readyButtonTextLabel;

    public override void OnNetworkSpawn()
    {
        leaveLobbyButton.onClick.AddListener(Disconnect);
        updatePlayerListButton.onClick.AddListener(LobbyManager.Instance.ListPlayers);
        readyButton.onClick.AddListener(ChangeReady);
        readyButtonTextLabel = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        readyButtonTextLabel.text = notReadyText;
        //LobbyManager.Instance.OnPlayerListed.AddListener(UpdatePlayerList);
        LobbyManager.Instance.ListPlayers();

        allPlayerReady.OnListChanged += AllPlayerReady_OnListChanged;
        playersId.OnListChanged += PlayersId_OnListChanged;

        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += PLayerLeave;
            NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnect;

            PlayerConnect(0);
        }
    }

    public override void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= PLayerLeave;
            NetworkManager.Singleton.OnClientConnectedCallback -= PlayerConnect;
        }
    }

    private void PlayersId_OnListChanged(NetworkListEvent<byte> changeEvent)
    {
        //Debug.LogError("PlayersId_OnListChanged");

        playerDataViewManager.AddPlayer(changeEvent.Value);

        foreach (GameObject player in playerGameObjectList)
        {
            player.SetActive(false);
        }
        for (int i = 0; i < playersId.Count; i++)
        {
            playerGameObjectList[i].SetActive(true);
        }
    }

    private void PlayerConnect(ulong clientId)
    {
        //Debug.LogError("Player conected");
        playersId.Add((byte)clientId);
        allPlayerReady.Add(false);
    }

    private void PLayerLeave(ulong clientId)
    {
        int clientIdIndex = GetClientIdIndex(clientId);

        if (clientIdIndex < 0)
        {
            Logger.Instance.LogWarning(this, "Client id index below 0");
            return;
        }

        playersId.RemoveAt(clientIdIndex);
        allPlayerReady.RemoveAt(clientIdIndex);
    }

    private int GetClientIdIndex(ulong id)
    {
        try
        {
            int i = 0;

            for (; i < playersId.Count; i++)
                if (playersId[i] == id)
                    break;

            return i;
        }
        catch (ObjectDisposedException ex)
        {
            Logger.Instance.LogError(this, ex);
        }

        return -1;
    }

    public void ChangeReady()
    {
        int clientIdIndex = GetClientIdIndex(NetworkManager.Singleton.LocalClientId);

        playerReady[clientIdIndex].SetActive(!allPlayerReady[clientIdIndex]);
        readyButtonTextLabel.text = playerReady[clientIdIndex].activeSelf ? readyText : notReadyText;

        SwitchPlayerReadyServerRpc(clientIdIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwitchPlayerReadyServerRpc(int clientIdIndex)
    {
        //Debug.LogError("Server");
        allPlayerReady[clientIdIndex] = !allPlayerReady[clientIdIndex];
    }

    private void AllPlayerReady_OnListChanged(NetworkListEvent<bool> changeEvent)
    {
        //Debug.LogError("AllPlayerReady_OnListChanged");
        playerReady[changeEvent.Index].SetActive(changeEvent.Value);

        if (NetworkManager.Singleton.IsServer)
        {
            int howManyPlayerReady = 0;
            bool allReady = true;
            for (int i = 0; i < allPlayerReady.Count; i++)
            {
                if (!allPlayerReady[i])
                {
                    if (playerGameObjectList[i].activeSelf)
                    {
                        allReady = false;
                    }
                }
                else
                {
                    howManyPlayerReady += 1;
                }
            }
            
            if (allReady && howManyPlayerReady >= 2)
            {
                LobbyManager.Instance.StopHeartBeatPing();
                SceneLoader.ServerLoad("Map_New");
            }
        }
    }

    private async void Disconnect()
    {
        RelayManager.Instance.Disconnect();
        await LobbyManager.Instance.DisconnectAsync();
    }
}