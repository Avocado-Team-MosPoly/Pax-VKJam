using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManagerUI : NetworkBehaviour
{
    //[SerializeField] private Button listPlayersButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button updatePlayerList;
    [SerializeField] private Button ready;
    [SerializeField] private List<GameObject> playerGameObjectList;
    [SerializeField] private List<GameObject> playerReady;

    //[SerializeField] private RectTransform playerListContainer;
    //[SerializeField] private GameObject playerInfoPrefab;
    [SerializeField] private NetworkList<bool> allPlayerReady = new();
    private NetworkList<ulong> playersId = new();

    public override void OnNetworkSpawn()
    {
        leaveLobbyButton.onClick.AddListener(Disconnect);
        updatePlayerList.onClick.AddListener(LobbyManager.Instance.ListPlayers);
        ready.onClick.AddListener(ChangeReady);

        //LobbyManager.Instance.OnPlayerListed.AddListener(UpdatePlayerList);
        LobbyManager.Instance.ListPlayers();

        allPlayerReady.OnListChanged += AllPlayerReady_OnListChanged;
        playersId.OnListChanged += PlayersId_OnListChanged;

        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += PLayerLeave;
            NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnect;

            NetworkManager.Singleton.OnServerStopped += OnServerStopped_OnHost;

            PlayerConnect(0);
        }
    }

    private void OnServerStopped_OnHost(bool obj)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= PLayerLeave;
        NetworkManager.Singleton.OnClientConnectedCallback -= PlayerConnect;
    }

    private void PlayersId_OnListChanged(NetworkListEvent<ulong> changeEvent)
    {
        //Debug.LogError("PlayersId_OnListChanged");
        foreach (GameObject player in playerGameObjectList)
        {
            player.SetActive(false);
        }
        for (int i = 0; i < playersId.Count; i++)
        {
            playerGameObjectList[i].SetActive(true);
        }
    }

    private void PlayerConnect(ulong obj)
    {
        //Debug.LogError("Player conected");
        playersId.Add(obj);
        allPlayerReady.Add(false);
    }

    private void PLayerLeave(ulong clientId)
    {
        allPlayerReady[GetClientIdIndex(clientId)] = false;
        playersId.RemoveAt(GetClientIdIndex(clientId));
    }

    private int GetClientIdIndex(ulong id)
    {
        int i = 0;
        for (; i < playersId.Count; i++)
        {
            if (playersId[i] == id)
            {               
                break;
            }
        }
        return i;
    }

    public void ChangeReady()
    {
        UpdatePlayerReadyServerRpc(GetClientIdIndex(NetworkManager.Singleton.LocalClientId));
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerReadyServerRpc(int clientIdIndex)
    {
        //Debug.LogError("Server");
        allPlayerReady[clientIdIndex] = !allPlayerReady[clientIdIndex];
    }

    private void AllPlayerReady_OnListChanged(NetworkListEvent<bool> changeEvent)
    {
        //Debug.LogError("AllPlayerReady_OnListChanged");
        for (int i = 0; i < allPlayerReady.Count; i++)
        {
            playerReady[i].SetActive(allPlayerReady[i]);
        }
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
        await LobbyManager.Instance.LeaveLobbyAsync();
    }
}