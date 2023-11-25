using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using System.Threading.Tasks;
using UnityEditor.PackageManager;

public class LobbyManager : MonoBehaviour
{
    public Lobby CurrentLobby { get; private set; }

    private bool isSendHeartBeatPing = false;
    private float heartBeatTime = 15f;
    private float heartBeatTimer;

    [HideInInspector] public UnityEvent<List<Lobby>> OnLobbyListed = new();
    [HideInInspector] public UnityEvent<List<Player>> OnPlayerListed = new();

    public readonly string KEY_RELAY_CODE = "RelayCode";
    public readonly string KEY_TEAM_MODE = "IsTeamMode";
    public readonly string KEY_ROUND_AMOUNT = "RoundAmount";
    public readonly string KEY_TIMER_AMOUNT = "TimerAmount";
    public readonly string KEY_RECIPE_MODE = "RecipeMode";

    public bool IsServer => NetworkManager.Singleton.IsServer;
    public string LobbyName => CurrentLobby != null ? CurrentLobby.Name : "Íå èçâåñòíî";
    public string LobbyPlayerId { get; private set; }
    public static LobbyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance.gameObject);
        }
        else
        {
            Destroy(this);
            return;
        }
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) => RemovePlayer(clientId);
        NetworkManager.Singleton.OnClientStopped += (bool someBool) => LeaveLobbyAsync();
        NetworkManager.Singleton.OnServerStopped += (bool isHostLeave) => OnServerEnded();
        //NetworkManager.Singleton.OnServerStarted += StopHeartBeatPing;
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) =>
        {
            Logger.Instance.Log("Client Connected");
            if (clientId != NetworkManager.Singleton.LocalClientId)
                ListPlayers();
        };

        Authentication.Authenticate();
    }


    private void Update()
    {
        HeartBeatPing();
    }

    #region HeartBeat

    private async void HeartBeatPing()
    {
        if (!isSendHeartBeatPing || !IsServer || CurrentLobby == null)
            return;

        heartBeatTimer += Time.deltaTime;
        if (heartBeatTimer >= heartBeatTime)
        {
            heartBeatTimer = 0f;
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            }
            catch (LobbyServiceException ex)
            {
                Logger.Instance.Log(ex);
            }
        }
    }

    public void StartHeartBeatPing()
    {
        isSendHeartBeatPing = true;
        heartBeatTimer = heartBeatTime;
    }

    public void StopHeartBeatPing()
    {
        isSendHeartBeatPing = false;
    }

    #endregion

    public async Task UpdateLocalLobbyData()
    {
        if (IsServer)
            await UpdateLocalLobbyDataClientRpc();
        else
            await UpdateLocalLobbyDataServerRpcAsync();
    }

    [ServerRpc(RequireOwnership = false)]
    private async Task UpdateLocalLobbyDataServerRpcAsync()
    {
        await UpdateLocalLobbyDataClientRpc();
    }

    [ClientRpc]
    private async Task UpdateLocalLobbyDataClientRpc()
    {
        try
        {
            CurrentLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            Logger.Instance.Log($"[{nameof(LobbyManager)}] Local Lobby Data Updated");
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }

    private Dictionary<string, DataObject> GetLobbyData()
    {
        Dictionary<string, DataObject> lobbyData;

        if (LobbyDataInput.Instance)
        {
            lobbyData = new Dictionary<string, DataObject>
            {
                { KEY_TEAM_MODE, new DataObject(DataObject.VisibilityOptions.Public, LobbyDataInput.Instance.GameMode.ToString()) },
                { KEY_ROUND_AMOUNT, new DataObject(DataObject.VisibilityOptions.Public, LobbyDataInput.Instance.RoundAmount.ToString()) },
                { KEY_TIMER_AMOUNT, new DataObject(DataObject.VisibilityOptions.Public, LobbyDataInput.Instance.TimerAmount.ToString()) },
                { KEY_RECIPE_MODE, new DataObject(DataObject.VisibilityOptions.Public, ((int)LobbyDataInput.Instance.RecipeMode).ToString()) },
            };
        }
        else
        {
            lobbyData = new Dictionary<string, DataObject>
            {
                { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                { KEY_TEAM_MODE, new DataObject(DataObject.VisibilityOptions.Public, "1") },
                { KEY_ROUND_AMOUNT, new DataObject(DataObject.VisibilityOptions.Public, "4") },
                { KEY_TIMER_AMOUNT, new DataObject(DataObject.VisibilityOptions.Public, "40") },
                { KEY_RECIPE_MODE, new DataObject(DataObject.VisibilityOptions.Public, "0") },
            };
        }

        return lobbyData;
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                //{ "Id", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "-1") },
                { "Player Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, Authentication.PlayerName) }
            }
        };
    }

    private void SaveRelayJoinCode(string relayJoinCode)
    {
        if (!IsServer && CurrentLobby == null)
            return;
        try
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) },
            }
            };

            LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateLobbyOptions);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }

    public void ResetManager(bool isServer)
    {
        CurrentLobby = null;
        LobbyPlayerId = string.Empty;

        if (isServer)
        {

        }
    }

    public async void CreateLobby()
    {
        try
        {
            if (CurrentLobby != null)
                return;

            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = LobbyDataInput.Instance.IsPrivate,
                Player = GetPlayer(),
                Data = GetLobbyData()
            };
           
            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync
            (
                LobbyDataInput.Instance.LobbyName == "" ? Authentication.PlayerName : LobbyDataInput.Instance.LobbyName,
                LobbyDataInput.Instance.MaxPlayers,
                createLobbyOptions
            );

            LobbyPlayerId = CurrentLobby.Players[CurrentLobby.Players.Count - 1].Id;

            StartHeartBeatPing();

            Logger.Instance.Log($"Created lobby: {CurrentLobby.Name}, max players: {CurrentLobby.MaxPlayers}, lobby code: {CurrentLobby.LobbyCode}");

            string relayJoinCode = await RelayManager.Instance.CreateRelay();
            if (relayJoinCode != "0")
                SaveRelayJoinCode(relayJoinCode);
            else
                Debug.LogError($"[{name}] Invalid relay join code");
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }


    public async void JoinLobby(string joinCode)
    {
        try
        {
            if (CurrentLobby != null)
                return;

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
            {
                Player = GetPlayer()
            };

            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, joinLobbyByCodeOptions);

            LobbyPlayerId = CurrentLobby.Players[CurrentLobby.Players.Count - 1].Id;

            Logger.Instance.Log("You joined lobby " + CurrentLobby.Name);

            RelayManager.Instance.JoinRelay(CurrentLobby.Data[KEY_RELAY_CODE].Value);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        try
        {
            if (CurrentLobby != null)
                return;

            JoinLobbyByIdOptions joinLobbyByIdOptions = new()
            {
                Player = GetPlayer()
            };

            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);

            Logger.Instance.Log("You joined lobby " + lobby.Name);
            
            RelayManager.Instance.JoinRelay(CurrentLobby.Data[KEY_RELAY_CODE].Value);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }

    public async Task LeaveLobbyAsync()
    {
        try
        {
            if (CurrentLobby == null)
                return;

            string playerId = AuthenticationService.Instance.PlayerId;

            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);

            Logger.Instance.Log($"You left the \"{CurrentLobby.Name}\" lobby");
            CurrentLobby = null;
            //LeaveRelay();
        }
        catch (LobbyServiceException ex)
        {
            if (ex.ErrorCode == 404)
                CurrentLobby = null;

            Logger.Instance.Log(ex);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            OnLobbyListed?.Invoke(queryResponse.Results);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }

    public async void ListPlayers()
    {
        Logger.Instance.Log("ListPlayers");

        try
        {
            Lobby currentLobbyUpdate = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            if (currentLobbyUpdate == null)
                return;

            CurrentLobby = currentLobbyUpdate;
            OnPlayerListed?.Invoke(CurrentLobby.Players);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }

    public async void RemovePlayer(ulong clientId)
    {
        if (!IsServer)
        {
            Debug.LogWarning($"[{nameof(LobbyManager)}] Remove a player can only the server");
            return;
        }

        if (CurrentLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, PlayersDataManager.Instance.PlayerDatas[clientId].AuthenticationServiceId);
            }
            catch (LobbyServiceException ex)
            {
                Logger.Instance.Log(ex);
            }
        }
        else
            Debug.LogWarning($"[{nameof(LobbyManager)}] You are not in the lobby");
    }

    public async void Disconnect()
    {
        if (CurrentLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            catch (LobbyServiceException ex)
            {
                Logger.Instance.Log(ex);
            }
        }
        else
            Debug.LogWarning($"[{nameof(LobbyManager)}] You are not in the lobby");
    }

    public void OnServerEnded()
    {
        //Debug.Log("On Server Ended");
        //playerUlongIdList.Clear();
        //LeaveLobbyAsync();
    }
}