using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using System.Threading.Tasks;
using System;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [HideInInspector] public UnityEvent<List<Lobby>> OnLobbyListed = new();
    [HideInInspector] public UnityEvent<List<Player>> OnPlayerListed = new();

    [HideInInspector] public string IsTeamMode = "True";

    public Lobby CurrentLobby { get; private set; }

    public bool IsServer => NetworkManager.Singleton.IsServer;
    public string LobbyName => CurrentLobby != null ? CurrentLobby.Name : string.Empty;
    public string LobbyPlayerId { get; private set; }

    public readonly string KEY_RELAY_CODE = "RelayCode";
    public readonly string KEY_TEAM_MODE = "IsTeamMode";
    public readonly string KEY_ROUND_AMOUNT = "RoundAmount";
    public readonly string KEY_TIMER_AMOUNT = "TimerAmount";
    public readonly string KEY_RECIPE_MODE = "RecipeMode";

    private bool isSendHeartBeatPing = false;
    private float heartBeatTime = 15f;
    private float heartBeatTimer;


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

        Authentication.Authenticate();
    }

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Logger.Instance.LogError(this, "NM is null");
        }

        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) => DisconnectPlayer(clientId);
        NetworkManager.Singleton.OnClientStopped += async (bool someBool) => await LeaveLobbyAsync();
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) =>
        {
            Logger.Instance.Log(this, "Client Connected");
            if (clientId != NetworkManager.Singleton.LocalClientId)
                ListPlayers();
        };

        IsTeamMode = "True";
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
                Logger.Instance.LogError(this, ex);
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
            Logger.Instance.Log(this, "Local Lobby Data Updated");
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
        }
    }

    private Dictionary<string, DataObject> GetLobbyData()
    {
        Dictionary<string, DataObject> lobbyData;

        if (LobbyDataInput.Instance)
        {
            lobbyData = new Dictionary<string, DataObject>
            {
                { KEY_TEAM_MODE, new DataObject(DataObject.VisibilityOptions.Public, LobbyDataInput.Instance.GameMode.ToString(), DataObject.IndexOptions.S1) },
                { KEY_ROUND_AMOUNT, new DataObject(DataObject.VisibilityOptions.Public, LobbyDataInput.Instance.RoundAmount.ToString()) },
                { KEY_TIMER_AMOUNT, new DataObject(DataObject.VisibilityOptions.Public, LobbyDataInput.Instance.TimerAmount.ToString()) },
                { KEY_RECIPE_MODE, new DataObject(DataObject.VisibilityOptions.Public, ((int)LobbyDataInput.Instance.RecipeMode).ToString()) },
            };
        }
        else
        {
            // default lobby data
            lobbyData = new Dictionary<string, DataObject>
            {
                { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                { KEY_TEAM_MODE, new DataObject(DataObject.VisibilityOptions.Public, "True") },
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
                    { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            };

            LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateLobbyOptions);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
        }
    }

    public void ResetManager(bool isServer)
    {
        CurrentLobby = null;
        LobbyPlayerId = string.Empty;

        if (isServer)
        {
            ListLobbiesWithFilter();
        }
    }

    public async void CreateLobby()
    {
        try
        {
            if (CurrentLobby != null)
            {
                Logger.Instance.LogWarning(this, "You are already in lobby: " + CurrentLobby.Name);
                return;
            }

            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = LobbyDataInput.Instance.IsPrivate,
                Player = GetPlayer(),
                Data = GetLobbyData()
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync
            (
                lobbyName: string.IsNullOrEmpty(LobbyDataInput.Instance.LobbyName) ? Authentication.PlayerName : LobbyDataInput.Instance.LobbyName,
                maxPlayers: LobbyDataInput.Instance.MaxPlayers,
                options: createLobbyOptions
            );

            LobbyPlayerId = CurrentLobby.Players[CurrentLobby.Players.Count - 1].Id;

            StartHeartBeatPing();

            Logger.Instance.Log(this, $"Created lobby: {CurrentLobby.Name}, max players: {CurrentLobby.MaxPlayers}, lobby code: {CurrentLobby.LobbyCode}");

            string relayJoinCode = await RelayManager.Instance.CreateRelay();

            if (relayJoinCode != "0")
                SaveRelayJoinCode(relayJoinCode);
            else
            {
                Logger.Instance.LogError(this, new FormatException("Invalid relay join code"));
                Disconnect();
            }
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
            NotificationSystem.Instance.SendLocal("Can't connect to Unity Lobby servers");
        }
    }


    public async Task JoinLobbyByCodeAsync(string joinCode)
    {
        try
        {
            if (CurrentLobby != null)
            {
                Logger.Instance.LogWarning(this, "You are already in lobby: " + CurrentLobby.Name);
                return;
            }

            if (string.IsNullOrEmpty(joinCode))
            {
                Logger.Instance.LogError(this, new FormatException($"Invalid join code"));
                return;
            }

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
            {
                Player = GetPlayer()
            };

            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, joinLobbyByCodeOptions);

            Logger.Instance.Log(this, "You joined lobby " + CurrentLobby.Name);

            LobbyPlayerId = CurrentLobby.Players[CurrentLobby.Players.Count - 1].Id;

            if (await RelayManager.Instance.JoinRelay(CurrentLobby.Data[KEY_RELAY_CODE].Value))
                Disconnect();
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
            NotificationSystem.Instance.SendLocal("Can't connect to Unity Lobby servers");
        }
    }

    public async Task JoinLobbyByIdAsync(string lobbyId)
    {
        try
        {
            if (CurrentLobby != null)
            {
                Logger.Instance.LogWarning(this, "You are already in lobby: " + CurrentLobby.Name);
                return;
            }

            if (string.IsNullOrEmpty(lobbyId))
            {
                Logger.Instance.LogError(this, new FormatException($"Invalid join code"));
                return;
            }

            JoinLobbyByIdOptions joinLobbyByIdOptions = new()
            {
                Player = GetPlayer()
            };

            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByIdOptions);

            Logger.Instance.Log(this, "You joined lobby " + CurrentLobby.Name);

            LobbyPlayerId = CurrentLobby.Players[CurrentLobby.Players.Count - 1].Id;

            if (await RelayManager.Instance.JoinRelay(CurrentLobby.Data[KEY_RELAY_CODE].Value))
                Disconnect();
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
            NotificationSystem.Instance.SendLocal("Can't connect to Unity Lobby servers");
        }
    }

    public async Task JoinLobbyAsync(Lobby lobby)
    {
        await JoinLobbyByIdAsync(lobby.Id);
    }

    public async void QuickJoinLobby()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };
            QuickJoinLobbyOptions quickJoinLobbyOptions = new()
            {
                Player = GetPlayer(),
                Filter = new List<QueryFilter>()
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0")
                }

            };

            CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);

            Logger.Instance.Log(this, "You joined lobby " + CurrentLobby.Name);

            await RelayManager.Instance.JoinRelay(CurrentLobby.Data[KEY_RELAY_CODE].Value);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
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

            Logger.Instance.Log(this, $"You left the \"{CurrentLobby.Name}\" lobby");
            CurrentLobby = null;
            //LeaveRelay();
        }
        catch (LobbyServiceException ex)
        {
            if (ex.ErrorCode == 404)
                CurrentLobby = null;

            Logger.Instance.Log(this, ex);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };
            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(this, ex);
        }
    }
    public async void ListLobbiesWithFilter()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();

            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter
                (
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"
                ),
                new QueryFilter
                (
                    field: QueryFilter.FieldOptions.S1,
                    op: QueryFilter.OpOptions.EQ,
                    value: IsTeamMode
                )
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder>()
            {
                new QueryOrder
                (
                    asc: false,
                    field: QueryOrder.FieldOptions.Created
                )
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            OnLobbyListed?.Invoke(lobbies.Results);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
        }
    }

    public async void ListPlayers()
    {
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
            Logger.Instance.LogError(this, ex);
        }
    }

    public async void DisconnectPlayer(ulong clientId)
    {
        if (!IsServer)
        {
            Logger.Instance.LogWarning(this, "Remove a player can only server");
            return;
        }

        if (CurrentLobby != null)
        {
            try
            {
                string playerName = PlayersDataManager.Instance.PlayerDatas[clientId].Name;
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, PlayersDataManager.Instance.PlayerDatas[clientId].AuthenticationServiceId);
                Logger.Instance.Log(this, "Disconnected player " + playerName);
            }
            catch (LobbyServiceException ex)
            {
                Logger.Instance.LogError(this, ex);
            }
        }
        else
            Logger.Instance.LogWarning(this, "You are not in the lobby");
    }

    public async void Disconnect()
    {
        if (CurrentLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
                Logger.Instance.Log(this, "Disconnected from lobby: " + CurrentLobby.Name);
                CurrentLobby = null;
            }
            catch (LobbyServiceException ex)
            {
                Logger.Instance.LogError(this, ex);
            }
        }
        else
            Logger.Instance.LogWarning(this, "You are not in the lobby");
    }

    public void OnServerEnded()
    {
        //Debug.Log("On Server Ended");
        //playerUlongIdList.Clear();
        //LeaveLobbyAsync();
    }
}