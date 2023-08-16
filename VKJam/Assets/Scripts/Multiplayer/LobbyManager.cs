using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Netcode;
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private string playerName = "Lucifer";
    [SerializeField] private string gameSceneName;

    [SerializeField] private string lobbyName = "Paradise";
    [SerializeField] private int maxPlayers = 4;

    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button listLobbiesButton;
    [SerializeField] private Button listPlayersButton;

    [SerializeField] private Transform containerLobbyList;
    [SerializeField] private Transform lobbySingleTemplate;

    [SerializeField] private Transform containerPlayerList;
    [SerializeField] private Transform playerSingleTemplate;

    [SerializeField] private Transform lobbyCreation;
    [SerializeField] private Transform lobbyList;
    [SerializeField] private Transform lobby;
    [SerializeField] private Transform main;
    [SerializeField] private Transform shop;
    [SerializeField] private Transform startGameButton;
    private List<Transform> canvases;

    private Lobby currentLobby;

    private bool isSendHeartBeatPing = false;
    private float heartBeatTime = 15f;
    private float heartBeatTimer;

    private string KEY_START_GAME = "0";
    private string KEY_RELAY_CODE = "RelayCode";
    public readonly string KEY_TEAM_MODE = "IsTeamMode";
    public readonly string KEY_ROUND_AMOUNT = "RoundAmount";
    public readonly string KEY_RECIPE_MODE = "RecipeMode";

    public bool IsHost => NetworkManager.Singleton.IsHost;

    public static LobbyManager Instance { get; private set; }

    private void Awake()
    {
        canvases.Add(lobbyCreation);
        canvases.Add(lobbyList);
        canvases.Add(lobby);
        canvases.Add(main);
        canvases.Add(shop);

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
        if (!isSendHeartBeatPing || IsHost || currentLobby == null)
            return;

        heartBeatTimer += Time.deltaTime;
        if (heartBeatTimer >= heartBeatTime)
        {
            heartBeatTimer = 0f;
            await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
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

            currentLobby = lobby;
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

    private async void JoinLobby(string joinCode)
    {
        try
        {
            Logger.Instance.Log(joinCode);
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode);
            currentLobby = lobby;
             
            Logger.Instance.Log("You joined lobby " + lobby.Name);

            RelayManager.Instance.JoinRelay(currentLobby.Data[KEY_RELAY_CODE].Value);
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
            Logger.Instance.Log(lobby.Id);
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            
            Logger.Instance.Log("You joined lobby " + lobby.Name);

            RelayManager.Instance.JoinRelay(currentLobby.Data[KEY_RELAY_CODE].Value);
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
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);

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
        foreach (Transform child in containerLobbyList)
        {
            if (child == lobbySingleTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, containerLobbyList);
            lobbySingleTransform.gameObject.SetActive(true);
            LobbyListSingleUi lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUi>();
            lobbyListSingleUI.UpdateLobby(lobby);
        }
    }
    private void UpdatePlayerList()
    {
        foreach (Transform child in lobbySingleTemplate)
        {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Player player in currentLobby.Players)
        {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, lobbySingleTemplate);
            playerSingleTransform.gameObject.SetActive(true);
            PlayerListSingleTemplate playerListSingleTemplate = playerSingleTransform.GetComponent<PlayerListSingleTemplate>();
            playerListSingleTemplate.UpdatePlayer(player);
        }
    }

    private void ListPlayers()
    {
        Logger.Instance.Log("Players list in lobby " + currentLobby.Name + ":");
        foreach (Player player in currentLobby.Players)
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

        LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateLobbyOptions);
    }



    public void clickMain()
    {
        foreach (Transform canvas in canvases)
        {
            canvas.gameObject.SetActive(false);
        }
        if (currentLobby != null)
            LeaveLobby();
        main.gameObject.SetActive(true);
    }

    public void clickLobbeList()
    {
        foreach (Transform canvas in canvases)
        {
            canvas.gameObject.SetActive(false);
        }
        lobbyList.gameObject.SetActive(true);
    }

    public void clickCreateLobbe()
    {
        foreach (Transform canvas in canvases)
        {
            canvas.gameObject.SetActive(false);
        }
        lobbyCreation.gameObject.SetActive(true);        
    }

    public void clickGoToLobby()
    {
        foreach (Transform canvas in canvases)
        {
            canvas.gameObject.SetActive(false);
        }
       lobby.gameObject.SetActive(true);
        if (IsHost)
        {
            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
        }
    }

    public void clickShop()
    {
        foreach (Transform canvas in canvases)
        {
            canvas.gameObject.SetActive(false);
        }
        shop.gameObject.SetActive(true);       
    }

    public void clickStartGame()
    {
        SceneLoader.ServerLoad(gameSceneName);
    }
}