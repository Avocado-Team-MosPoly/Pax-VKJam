using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private Button callFriendsToLobbyButton;

    [SerializeField] private List<GameObject> playerGameObjectList = new();
    [SerializeField] private List<GameObject> playerReady = new();
    [SerializeField] private LobbyPlayerDataViewManager playerDataViewManager;

    [SerializeField] private string notReadyText;
    [SerializeField] private string readyText;

    private List<ulong> playerSendAllDataId=new();
    private List<ulong> playerGetAllDataId = new();
    private ulong sendedPlayerId = new();


    //[SerializeField] private RectTransform playerListContainer;
    //[SerializeField] private GameObject playerInfoPrefab;
    [SerializeField] private NetworkList<bool> allPlayerReady = new();

    private TextMeshProUGUI readyButtonTextLabel;

    [SerializeField] private TextMeshProUGUI waitingLabel;
    [SerializeField] private string LoadingText;
    //[SerializeField] private Slider loadingSlider;

    [SerializeField] private GameObject exit;
    [SerializeField] private GameObject refresh;
    [SerializeField] private GameObject ready;

    private void Start()
    {
        CheckReady();
    }

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
            foreach (byte clientId in NetworkManager.ConnectedClientsIds) // implicit conversion ulong to byte
                if (PlayersId.IndexOf(clientId) < 0)
                    PlayerConnect(clientId);

            NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += PLayerLeave;
        }
        else
            StartCoroutine(GetPlayerDataOnClient());
    }

    private IEnumerator GetPlayerDataOnClient()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (byte clientId in PlayersId)
            playerDataViewManager.AddPlayer(clientId);
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

        Debug.LogWarning(allPlayerReady[clientIdIndex]);

        playerReady[clientIdIndex].SetActive(!allPlayerReady[clientIdIndex]);
        readyButtonTextLabel.text = !allPlayerReady[clientIdIndex] ? readyText : notReadyText;
        
        SwitchPlayerReadyServerRpc(clientIdIndex);  
    }

    public void CheckReady()
    {
        for (int i = 0; i < allPlayerReady.Count; i++)
        {
            playerReady[i].SetActive(allPlayerReady[i]);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwitchPlayerReadyServerRpc(int clientIdIndex)
    {
        //Debug.LogError("Server");
        allPlayerReady[clientIdIndex] = !allPlayerReady[clientIdIndex];
        Debug.LogWarning(allPlayerReady[clientIdIndex]);
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
                        Debug.LogWarning("Not all players ready");
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
                playerDataViewManager.LockKick();
                SendPack();
            }
        }
    }

    private void SendPack()
    {
        List<bool> bools = new List<bool>();
        for (int i = 0; i < PackManager.Instance.Active.CardInPack.Length; i++)
        {
            bools.Add(PackManager.Instance.Active.CardInPack[i].CardIsInOwn);
        }

        PackManager.Instance.PlayersOwnedCard.Clear();
        PackManager.Instance.PlayersOwnedCard.Add(NetworkManager.LocalClientId, bools);

        playerSendAllDataId.Add(NetworkManager.LocalClientId);
        playerGetAllDataId.Add(NetworkManager.LocalClientId);
        //Debug.Log("SendPack");
        PrepareToSendDataClientRpc();
        SendToHostClientRpc();
    }

    [ClientRpc]
    private void PrepareToSendDataClientRpc()
    {
        callFriendsToLobbyButton.gameObject.SetActive(false);
        //waitingLabel.gameObject.SetActive(false);
        //loadingSlider.gameObject.SetActive(true);
        waitingLabel.text = LoadingText;
        exit.SetActive(false);
        refresh.SetActive(false);
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
            for (int i = 0; i < PackManager.Instance.Active.CardInPack.Length; i++)
            {
                bools.Add(PackManager.Instance.Active.CardInPack[i].CardIsInOwn);
            }

            PackManager.Instance.PlayersOwnedCard.Clear();
            PackManager.Instance.PlayersOwnedCard.Add(NetworkManager.LocalClientId, bools);

            //Debug.Log("SendToHostClientRpc");

            for (int i = 0; i < bools.Count; i++)
            {
                try
                {
                    GetPackFromPlayersServerRpc(bools[i], new ServerRpcParams());
                }
                catch (Exception e)
                {
                    exit.SetActive(true);
                    Logger.Instance.LogError(this, e);
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            try
            {
                SendAllServerRpc(new ServerRpcParams());
            }
            catch (Exception e)
            {
                exit.SetActive(true);
                Logger.Instance.LogError(this, e);
            }

        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void GetPackFromPlayersServerRpc(bool SendBool, ServerRpcParams serverRpcParams)
    {
        try
        {
            if (!PackManager.Instance.PlayersOwnedCard.ContainsKey(serverRpcParams.Receive.SenderClientId))
            {
                PackManager.Instance.PlayersOwnedCard.Add(serverRpcParams.Receive.SenderClientId, new List<bool>());
            }
            PackManager.Instance.PlayersOwnedCard[serverRpcParams.Receive.SenderClientId].Add(SendBool);
            //loadingSlider.value += 1f / PlayersId.Count * 35;
            //Logger.Instance.Log(this, loadingSlider.value);
        }
        catch (Exception e)
        {
            exit.SetActive(true);
            Logger.Instance.LogError(this, e);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendAllServerRpc(ServerRpcParams serverRpcParams)
    {
        try
        {
            //Debug.Log("SendAll");
            playerSendAllDataId.Add(serverRpcParams.Receive.SenderClientId);
            if (playerSendAllDataId.Count == allPlayerReady.Count)
            {
                SendPackToPlayersServerRpc();
            }
        }
        catch (Exception e)
        {
            exit.SetActive(true);
            Logger.Instance.LogError(this, e);
        }

    }

    [ServerRpc]
    private void SendPackToPlayersServerRpc()
    {
        StartCoroutine(SendPackToPlayers());
    }

    private IEnumerator SendPackToPlayers()
    {
        //Debug.Log("SendPackToPlayersServerRpc");
        for (int i = 0; i < PackManager.Instance.PlayersOwnedCard.Count; i++)
        {
            try
            {
                SetPlayerIdOfSendPackClientRpc(playerSendAllDataId[i]);
            }
            catch (Exception e)
            {
                exit.SetActive(true);
                Logger.Instance.LogError(this, e);
            }

            yield return new WaitForSeconds(0.1f);
            for (int j = 0; j < PackManager.Instance.PlayersOwnedCard[playerSendAllDataId[i]].Count; j++)
            {
                try
                {
                    GetPackFromHostClientRpc(PackManager.Instance.PlayersOwnedCard[playerSendAllDataId[i]] [j]);
                }
                catch (Exception e)
                {
                    exit.SetActive(true);
                    Logger.Instance.LogError(this, e);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    [ClientRpc]
    private void SetPlayerIdOfSendPackClientRpc(ulong Sendid)
    {
        try
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
        catch (Exception e)
        {
            exit.SetActive(true);
            Logger.Instance.LogError(this, e);
        }
    }

    [ClientRpc]
    private void GetPackFromHostClientRpc(bool SendBool)
    {
        try
        {
            //Debug.Log("GetPackFromHostClientRpc");
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
        catch (Exception e)
        {
            exit.SetActive(true);
            Logger.Instance.LogError(this, e);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetAllServerRpc(ServerRpcParams serverRpcParams)
    {
        try
        {
            Debug.Log("GetAllServerRpc");
            //for (int i = 0; i < PackManager.Instance.PlayersOwnedCard.Count; i++)
            //{
            //    Debug.Log("PlayersOwnedCard[PlayersId[" + i + "]].Count =" + PackManager.Instance.PlayersOwnedCard[PlayersId[i]].Count);
            //}
            playerGetAllDataId.Add(serverRpcParams.Receive.SenderClientId);
            //loadingSlider.value = playerGetAllDataId.Count / allPlayerReady.Count;

            if (playerGetAllDataId.Count == allPlayerReady.Count)
            {
                LobbyManager.Instance.StopHeartBeatPing();
                SceneLoader.ServerLoad("Map_New");
            }
        }
        catch (Exception e)
        {
            exit.SetActive(true);
            Logger.Instance.LogError(this, e);
        }

    }

    private void Disconnect()
    {
        RelayManager.Instance.Disconnect();
        //await LobbyManager.Instance.DisconnectAsync();
    }
}