using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManagerUI : NetworkBehaviour
{
    public NetworkList<byte> PlayersId = new(); // tie to PlayerDatasManager

    //[SerializeField] private Button listPlayersButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button updatePlayerListButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private List<GameObject> playerGameObjectList = new();
    [SerializeField] private List<GameObject> playerReady = new();
    [SerializeField] private LobbyPlayerDataViewManager playerDataViewManager;

    [SerializeField] private string notReadyText;
    [SerializeField] private string readyText;
    [SerializeField] private PackCardSO packCardSO;

    private List<ulong> playerSendAllDataId=new();
    private List<ulong> playerGetAllDataId = new();
    private ulong sendedPlayerId = new();


    //[SerializeField] private RectTransform playerListContainer;
    //[SerializeField] private GameObject playerInfoPrefab;
    [SerializeField] private NetworkList<bool> allPlayerReady = new();

    private TextMeshProUGUI readyButtonTextLabel;
    [SerializeField] private TextMeshProUGUI loading;
    [SerializeField] private GameObject exit;
    [SerializeField] private GameObject refresh;
    [SerializeField] private GameObject ready;


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
        PlayersId.OnListChanged += PlayersId_OnListChanged;

        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += PLayerLeave;

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
        if (changeEvent.Type == NetworkListEvent<byte>.EventType.Add)
            playerDataViewManager.AddPlayer(changeEvent.Value);
        if (changeEvent.Type == NetworkListEvent<byte>.EventType.RemoveAt)
            playerDataViewManager.RemovePlayer(changeEvent.Value);
    }

    private void PlayerConnect(ulong clientId)
    {
        //Debug.LogError("Player conected");
        PlayersId.Add((byte)clientId);
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

        PlayersId.RemoveAt(clientIdIndex);
        allPlayerReady.RemoveAt(clientIdIndex);
    }

    private int GetClientIdIndex(ulong id)
    {
        try
        {
            int i = 0;

            for (; i < PlayersId.Count; i++)
                if (PlayersId[i] == id)
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
                SendPack();
            }
        }
    }

    private void SendPack()
    {
        List<bool> bools = new List<bool>();
        for (int i = 0; i < packCardSO.CardInPack.Length; i++)
        {
            bools.Add(packCardSO.CardInPack[i].CardIsInOwn);            
        }
        PackManager.Instance.PlayersOwnedCard.Clear();       
        PackManager.Instance.PlayersOwnedCard.Add(NetworkManager.LocalClientId, bools);

        playerSendAllDataId.Add(NetworkManager.LocalClientId);
        playerGetAllDataId.Add(NetworkManager.LocalClientId);
        Debug.Log("SendPack");
        PrepareToSandDataClientRpc();
        SendToHostClientRpc();
    }
    [ClientRpc]
    private void PrepareToSandDataClientRpc()
    {
        loading.text ="Загрузка";
        exit.SetActive(false) ;
        refresh.SetActive(false) ;
        ready.SetActive(false);
    }

    [ClientRpc]
    private void SendToHostClientRpc()
    {
        StartCoroutine(SendToHost());
    }
    private IEnumerator SendToHost()
    {
        if (!IsServer)
        {
            List<bool> bools = new List<bool>();
            for (int i = 0; i < packCardSO.CardInPack.Length; i++)
            {
                bools.Add(packCardSO.CardInPack[i].CardIsInOwn);
            }
            PackManager.Instance.PlayersOwnedCard.Clear();
            PackManager.Instance.PlayersOwnedCard.Add(NetworkManager.LocalClientId, bools);

            Debug.Log("SendToHostClientRpc");

            for (int i = 0; i < bools.Count; i++)
            {
                GetPackFromPlayersServerRpc(bools[i], new ServerRpcParams());
                yield return new WaitForSeconds(0.1f);
            }
            SendAllServerRpc(new ServerRpcParams());
        }

    }
    [ServerRpc(RequireOwnership = false)]
    private void GetPackFromPlayersServerRpc(bool SendBool, ServerRpcParams serverRpcParams)
    {
        if (!PackManager.Instance.PlayersOwnedCard.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            PackManager.Instance.PlayersOwnedCard.Add(serverRpcParams.Receive.SenderClientId, new List<bool>());
        }
        PackManager.Instance.PlayersOwnedCard[serverRpcParams.Receive.SenderClientId].Add(SendBool);        
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendAllServerRpc(ServerRpcParams serverRpcParams)
    {
        Debug.Log("SendAll");
        playerSendAllDataId.Add(serverRpcParams.Receive.SenderClientId);
        if (playerSendAllDataId.Count == allPlayerReady.Count)
        {
            SendPackToPlayersServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPackToPlayersServerRpc()
    {
        StartCoroutine(SendPackToPlayers());
    }
    private IEnumerator SendPackToPlayers()
    {
        Debug.Log("SendPackToPlayersServerRpc");
        for (int i = 0; i < PackManager.Instance.PlayersOwnedCard.Count; i++)
        {
            SetPlayerIdOfSendPackClientRpc(playerSendAllDataId[i]);
            yield return new WaitForSeconds(0.1f);
            for (int io = 0; io < PackManager.Instance.PlayersOwnedCard[playerSendAllDataId[i]].Count; io++)
            {
                GetPackFromHostClientRpc(PackManager.Instance.PlayersOwnedCard[playerSendAllDataId[i]][io]);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    [ClientRpc]
    private void SetPlayerIdOfSendPackClientRpc(ulong Sendid)
    {
        sendedPlayerId = Sendid;
        if (!IsServer)
        {
            if (sendedPlayerId != NetworkManager.LocalClientId)
            {
                PackManager.Instance.PlayersOwnedCard.Add(sendedPlayerId, new List<bool>());
            }
        }

    }

    [ClientRpc]
    private void GetPackFromHostClientRpc(bool SendBool)
    {
        Debug.Log("GetPackFromHostClientRpc");
        if (!IsServer)
        {
            if (sendedPlayerId != NetworkManager.LocalClientId)
            {
                PackManager.Instance.PlayersOwnedCard[sendedPlayerId].Add(SendBool);
               
                if (PackManager.Instance.PlayersOwnedCard.Count == PlayersId.Count)
                {
                    bool allGet = true;
                    for (int i = 0; i < PackManager.Instance.PlayersOwnedCard.Count; i++)
                    {
                        if (PackManager.Instance.PlayersOwnedCard[PlayersId[i]].Count != PackManager.Instance.PlayersOwnedCard[NetworkManager.LocalClientId].Count)
                        {
                            allGet = false;
                        }
                    }
                    if (allGet)
                    {
                        GetAllServerRpc(new ServerRpcParams());
                    }
                }                   
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetAllServerRpc(ServerRpcParams serverRpcParams)
    {
        Debug.Log("GetAllServerRpc");
        for (int i = 0; i < PackManager.Instance.PlayersOwnedCard.Count; i++)
        {
            Debug.Log("PlayersOwnedCard[PlayersId[" + i + "]].Count =" + PackManager.Instance.PlayersOwnedCard[PlayersId[i]].Count);
        }
        playerGetAllDataId.Add(serverRpcParams.Receive.SenderClientId);
        if (playerGetAllDataId.Count == allPlayerReady.Count)
        {
            LobbyManager.Instance.StopHeartBeatPing();
            SceneLoader.ServerLoad("Map_New");
        }
    }

    private void Disconnect()
    {
        RelayManager.Instance.Disconnect();
        //await LobbyManager.Instance.DisconnectAsync();
    }
}