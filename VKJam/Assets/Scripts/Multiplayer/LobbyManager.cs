using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [HideInInspector] public UnityEvent<List<Lobby>> OnLobbyListed = new();
    [HideInInspector] public UnityEvent<List<Player>> OnPlayerListed = new();

    [HideInInspector] public int IsTeamModeFilter
    {
        get
        {
            return isTeamModeFilter;
        }
        set
        {
            isTeamModeFilter = value;
            ListLobbiesWithFilter();
        }
    }
    private int isTeamModeFilter = 0;

    public Lobby CurrentLobby { get; private set; }

    public bool IsServer => NetworkManager.Singleton.IsServer;
    public string LobbyName => CurrentLobby != null ? CurrentLobby.Name : string.Empty;
    public string PlayerId => AuthenticationService.Instance.PlayerId;

    public readonly string KEY_RELAY_CODE = "RelayCode";
    public readonly string KEY_TEAM_MODE = "IsTeamMode";
    public readonly string KEY_ROUND_AMOUNT = "RoundAmount";
    public readonly string KEY_TIMER_AMOUNT = "TimerAmount";
    public readonly string KEY_RECIPE_MODE = "RecipeMode";

    private bool isSendHeartBeatPing = false;
    private const float heartBeatTime = 15f;
    private const string errorText = "Ошибка соединения: Не получилось подключиться к серверам Unity Lobby";
    private float heartBeatTimer;

    public IEnumerator Init()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance.gameObject);
        }
        else
        {
            Destroy(this);
            yield break;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) =>
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
                ListPlayers();
        };

        //NetworkManager.Singleton.OnClientDisconnectCallback += async (ulong clientId) =>
        //{
        //    if (clientId == NetworkManager.Singleton.LocalClientId)
        //        await DisconnectAsync();
        //};

        NetworkManager.Singleton.OnClientStopped += async (bool isHost) =>
        {
            await DisconnectAsync();
        };

        Logger.Instance.Log(this, "Initialized");
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
                Logger.Instance.Log(this, "Send HeartbeatPing");
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

        Logger.Instance.Log(this, "Start HeartBeat Ping");
    }

    public void StopHeartBeatPing()
    {
        isSendHeartBeatPing = false;

        Logger.Instance.Log(this, "Stop HeartBeat Ping");
    }

    #endregion

    private bool CheckCurrentLobbyIsNull()
    {
        if (CurrentLobby == null)
            return true;

        NotificationSystem.Instance.SendLocal("Вы уже в лобби: " + CurrentLobby.Name);
        return false;
    }

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

    public void ResetManager()
    {
        CurrentLobby = null;
    }

    public async void CreateLobby()
    {
        try
        {
            if (!CheckCurrentLobbyIsNull())
                return;

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

            Logger.Instance.Log(this, $"Created lobby: {CurrentLobby.Name}, max players: {CurrentLobby.MaxPlayers}, lobby code: {CurrentLobby.LobbyCode}");

            StartHeartBeatPing();

            string relayJoinCode = await RelayManager.Instance.CreateRelay();

            if (relayJoinCode != "0")
                SaveRelayJoinCode(relayJoinCode);
            else
            {
                Logger.Instance.LogError(this, new FormatException("Invalid relay join code"));
                await DisconnectAsync();
            }
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
            NotificationSystem.Instance.SendLocal(errorText);
        }
    }

    public async Task JoinLobbyByCodeAsync(string joinCode)
    {
        try
        {
            if (!CheckCurrentLobbyIsNull())
                return;

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

            if (!await RelayManager.Instance.JoinRelay(CurrentLobby.Data[KEY_RELAY_CODE].Value))
                await DisconnectAsync();
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
            NotificationSystem.Instance.SendLocal(errorText);
        }
    }

    public async Task JoinLobbyByIdAsync(string lobbyId)
    {
        try
        {
            if (!CheckCurrentLobbyIsNull())
                return;

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

            if (!await RelayManager.Instance.JoinRelay(CurrentLobby.Data[KEY_RELAY_CODE].Value))
                await DisconnectAsync();
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
            NotificationSystem.Instance.SendLocal(errorText);
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
            if (!CheckCurrentLobbyIsNull())
                return;

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

            if (!await RelayManager.Instance.JoinRelay(CurrentLobby.Data[KEY_RELAY_CODE].Value))
                await DisconnectAsync();
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
            NotificationSystem.Instance.SendLocal(errorText);
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
            NotificationSystem.Instance.SendLocal(errorText);
        }
    }

    public async void ListLobbiesWithFilter()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();

            if(IsTeamModeFilter==0)
            {
                QueryLobbiesOptions options0 = new QueryLobbiesOptions();

                options0.Filters = new List<QueryFilter>()
                {
                    new QueryFilter
                    (
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"
                    )
                };
                options = options0;
            }
            else
            {
                QueryLobbiesOptions options1 = new QueryLobbiesOptions();

                options1.Filters = new List<QueryFilter>()
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
                        value: (IsTeamModeFilter == 1).ToString()
                    )
                };
                options = options1;
            }


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
            NotificationSystem.Instance.SendLocal(errorText);
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

    public async void DisconnectPlayerAsync(ulong clientId)
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
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, PlayersDataManager.Instance.PlayerDatas[clientId].LobbyPlayerId);
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

    public async Task DisconnectAsync()
    {
        if (CurrentLobby != null)
        {
            try
            {
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);

                foreach (Player player in lobby.Players)
                {
                    if (player.Id == PlayerId)
                    {
                        await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, PlayerId);
                        Logger.Instance.Log(this, $"You left the \"{CurrentLobby.Name}\" lobby");
                    }
                }

                ResetManager();
            }
            catch (LobbyServiceException ex)
            {
                ResetManager();
                Logger.Instance.LogError(this, ex);
            }
        }
        else
            Logger.Instance.LogWarning(this, "You are not in the lobby");
    }
}