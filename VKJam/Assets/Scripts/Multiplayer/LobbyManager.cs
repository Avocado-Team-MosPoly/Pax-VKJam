using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Netcode;

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
    public readonly string KEY_RECIPE_MODE = "RecipeMode";

    Dictionary<ulong, string> playerUlongIdList = new();

    public bool IsHost => NetworkManager.Singleton.IsHost;
    public string LobbyName => CurrentLobby != null ? CurrentLobby.Name : "Íå èçâåñòíî";

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
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) => KickPlayer(clientId);
        NetworkManager.Singleton.OnClientStopped += (bool someBool) => LeaveLobby();
        NetworkManager.Singleton.OnServerStopped += (bool isHostLeave) => OnServerEnded();
        NetworkManager.Singleton.OnServerStarted += StopHeartBeatPing;
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) => ListPlayers();
        if (IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) => playerUlongIdList.Add(clientId, AuthenticationService.Instance.PlayerId);
        }
        Authentication.Authenticate();
    }


    private void Update()
    {
        HeartBeatPing();
    }

    #region HeartBeat

    private async void HeartBeatPing()
    {
        if (!isSendHeartBeatPing || !IsHost || CurrentLobby == null)
            return;

        heartBeatTimer += Time.deltaTime;
        if (heartBeatTimer >= heartBeatTime)
        {
            heartBeatTimer = 0f;
            await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
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

    private Dictionary<string, DataObject> GetLobbyData()
    {
        Dictionary<string, DataObject> lobbyData;

        if (LobbyDataInput.Instance)
        {
            lobbyData = new Dictionary<string, DataObject>
            {
            };

            lobbyData[KEY_TEAM_MODE] = new(DataObject.VisibilityOptions.Public, LobbyDataInput.Instance.GameMode.ToString());
            lobbyData[KEY_ROUND_AMOUNT] = new(DataObject.VisibilityOptions.Public, LobbyDataInput.Instance.RoundAmount.ToString());
            lobbyData[KEY_RECIPE_MODE] = new(DataObject.VisibilityOptions.Public, ((int)LobbyDataInput.Instance.RecipeMode).ToString());
        }
        else
        {
            lobbyData = new Dictionary<string, DataObject>
            {
                { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                { KEY_TEAM_MODE, new DataObject(DataObject.VisibilityOptions.Public, "1") },
                { KEY_ROUND_AMOUNT, new DataObject(DataObject.VisibilityOptions.Public, "4") },
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

    private void SaveRelayCode(string relayJoinCode)
    {
        if (!IsHost && CurrentLobby == null)
            return;

        UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) },
            }
        };

        LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateLobbyOptions);
    }

    public async void CreateLobby()
    {
        try
        {
            if (CurrentLobby != null)
                return;

            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = GetLobbyData()
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync
            (
                LobbyDataInput.Instance.LobbyName == "" ? Authentication.PlayerName : LobbyDataInput.Instance.LobbyName,
                LobbyDataInput.Instance.MaxPlayers,
                createLobbyOptions
            );

            StartHeartBeatPing();

            Logger.Instance.Log($"Created lobby: {CurrentLobby.Name}, max players: {CurrentLobby.MaxPlayers}, lobby code: {CurrentLobby.LobbyCode}");

            string relayJoinCode = await RelayManager.Instance.CreateRelay();
            SaveRelayCode(relayJoinCode);
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

    public async void LeaveLobby()
    {
        try
        {
            if (CurrentLobby == null)
                return;

            string playerId = AuthenticationService.Instance.PlayerId;
            
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);

            LeaveRelay();
        }
        catch (LobbyServiceException ex)
        {
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
        Lobby currentLobbyUpdate = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);

        if (currentLobbyUpdate.Players.Count > CurrentLobby.Players.Count)
            CurrentLobby = currentLobbyUpdate;

        OnPlayerListed?.Invoke(CurrentLobby.Players);
    }

    public void LeaveRelay()
    {
        NetworkManager.Singleton.Shutdown();
        SceneLoader.Load("Menu");
    }
    public void KickPlayer(ulong client)
    {
        if (IsHost)
        {
            LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerUlongIdList[client]);
            NetworkManager.Singleton.DisconnectClient(client);
            SceneLoader.Load("Menu");
            LeaveLobby();
        }
    }
    public void OnServerEnded()
    {
        playerUlongIdList.Clear();
        LeaveLobby();
    }
}