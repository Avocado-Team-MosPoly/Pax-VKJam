using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Services.Authentication;
using System.ComponentModel;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private string playerName = "Lucifer";

    [SerializeField] private string lobbyName = "Paradise";
    [SerializeField] private int maxPlayers = 4;

    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button listLobbiesButton;
    [SerializeField] private Button listPlayersButton;

    [SerializeField] private Transform container;
    [SerializeField] private Transform lobbySingleTemplate;

    private Lobby hostLobby;
    private Lobby joinedLobby;

    private bool isSendHeartBeatPing = false;
    private float heartBeatTime = 15f;
    private float heartBeatTimer;

    private string KEY_START_GAME = "0";
    private string KEY_RELAY_CODE = "RelayCode";
    public readonly string KEY_TEAM_MODE = "IsTeamMode";
    public readonly string KEY_ROUND_AMOUNT = "RoundAmount";
    public readonly string KEY_RECIPE_MODE = "RecipeMode";

    public bool IsHost => hostLobby != null;

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
        }

        Authentication.Authenticate();

        createLobbyButton.onClick.AddListener(CreateLobby);
        //joinLobbyButton.onClick.AddListener( () => JoinLobbyByCode(LobbyDataInput.Instance.LobbyJoinCode) );
        listLobbiesButton.onClick.AddListener(ListLobbies);
        //listPlayersButton.onClick.AddListener(ListPlayers);
    }


    private void Update()
    {
        HeartBeatPing();
    }

    #region HeartBeat

    private async void HeartBeatPing()
    {
        if (!isSendHeartBeatPing || hostLobby == null)
            return;

        heartBeatTimer += Time.deltaTime;
        if (heartBeatTimer >= heartBeatTime)
        {
            heartBeatTimer = 0f;
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
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

    private async void CreateLobby()
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = GetLobbyData()
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(LobbyDataInput.Instance.LobbyName, LobbyDataInput.Instance.MaxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = lobby;
            StartHeartBeatPing();
            
            Logger.Instance.Log($"Created lobby: {lobby.Name}, max players: {lobby.MaxPlayers}, lobby code: {lobby.LobbyCode}");

            string relayJoinCode = await RelayManager.Instance.CreateRelay();
            SaveRelayCode(relayJoinCode);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }

    private async void JoinLobbyByCode(string joinCode)
    {
        try
        {
            Logger.Instance.Log(joinCode);
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode);
            joinedLobby = lobby;
             
            Logger.Instance.Log("You joined lobby " + lobby.Name);

            RelayManager.Instance.JoinRelay(lobby.Data[KEY_RELAY_CODE].Value);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }

    public async void JoinLobbyByLobby(Lobby lobby)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            joinedLobby = lobby;

            Logger.Instance.Log("You joined lobby " + lobby.Name);

            RelayManager.Instance.JoinRelay(lobby.Data[KEY_RELAY_CODE].Value);
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
            List <Lobby> lobbyList = queryResponse.Results;
            UpdateLobbyList(lobbyList);

            Logger.Instance.Log("Lobbies found: " + queryResponse.Results.Count);

            foreach (var lobby in queryResponse.Results)
            {
                Logger.Instance.Log(lobby.Name + " : " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in container)
        {
            if (child == lobbySingleTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
            lobbySingleTransform.gameObject.SetActive(true);
            LobbyListSingleUi lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUi>();
            lobbyListSingleUI.UpdateLobby(lobby);
        }
    }

    private void ListPlayers()
    {
        Logger.Instance.Log("Players list in lobby " + joinedLobby.Name + ":");
        foreach (Player player in joinedLobby.Players)
        {
            Logger.Instance.Log(player.Data["Player Name"].Value);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "Player Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }

    private Dictionary<string, DataObject> GetLobbyData()
    {
        Dictionary<string, DataObject> lobbyData;

        if (LobbyDataInput.Instance)
        {
            lobbyData = new Dictionary<string, DataObject>
            {
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") },
            };

            lobbyData[KEY_TEAM_MODE] = new(DataObject.VisibilityOptions.Public, LobbyDataInput.Instance.GameMode.ToString());
            lobbyData[KEY_ROUND_AMOUNT] = new(DataObject.VisibilityOptions.Public, LobbyDataInput.Instance.RoundAmount.ToString());
            lobbyData[KEY_RECIPE_MODE] = new(DataObject.VisibilityOptions.Public, ((int) LobbyDataInput.Instance.RecipeMode).ToString());
        }
        else
        {
            lobbyData = new Dictionary<string, DataObject>
            {
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                { KEY_TEAM_MODE, new DataObject(DataObject.VisibilityOptions.Public, "1") },
                { KEY_ROUND_AMOUNT, new DataObject(DataObject.VisibilityOptions.Public, "4") },
                { KEY_RECIPE_MODE, new DataObject(DataObject.VisibilityOptions.Public, "0") },
            };
        }

        return lobbyData;
    }

    public void SaveRelayCode(string relayJoinCode)
    {
        if (!IsHost)
            return;

        UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) },
            }
        };

        LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, updateLobbyOptions);
    }
}