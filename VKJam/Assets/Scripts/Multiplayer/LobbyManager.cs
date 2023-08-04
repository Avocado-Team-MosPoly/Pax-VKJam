using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Services.Authentication;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private string playerName = "Lucifer";

    [SerializeField] private string lobbyName = "Paradise";
    [SerializeField] private int maxPlayers = 4;

    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button listLobbiesButton;
    [SerializeField] private Button listPlayersButton;

    private Lobby hostLobby;
    private Lobby joinedLobby;

    private string KEY_START_GAME = "0";
    private string KEY_RELAY_CODE = "RelayCode";
    private string KEY_TEAM_MODE = "IsTeamMode";
    private string KEY_ROUND_AMOUNT = "RoundAmount";
    private string KEY_RECIPE_MODE = "RoundAmount";


    private void Start()
    {
        UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignInAnonymouslyAsync();

        createLobbyButton.onClick.AddListener(CreateLobby);
        //joinLobbyButton.onClick.AddListener(JoinLobby);
        listLobbiesButton.onClick.AddListener(ListLobbies);
        listPlayersButton.onClick.AddListener(ListPlayers);
    }

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

            Logger.Instance.Log($"Lobby created {lobby.Name}, max players: {lobby.MaxPlayers}, lobby code: {lobby.LobbyCode}");
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }

    private async void JoinLobbyByCode(string code)
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);

            Logger.Instance.Log("You joined lobby " + lobby.Name);
        }
        catch (LobbyServiceException ex)
        {
            Logger.Instance.Log(ex);
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

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

            lobbyData[KEY_TEAM_MODE] = new(DataObject.VisibilityOptions.Member, LobbyDataInput.Instance.IsTeamMode.ToString());
            lobbyData[KEY_ROUND_AMOUNT] = new(DataObject.VisibilityOptions.Member, LobbyDataInput.Instance.RoundAmount.ToString());
            lobbyData[KEY_RECIPE_MODE] = new(DataObject.VisibilityOptions.Member, ((int) LobbyDataInput.Instance.RecipeMode).ToString());
        }
        else
        {
            lobbyData = new Dictionary<string, DataObject>
            {
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                { KEY_TEAM_MODE, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                { KEY_ROUND_AMOUNT, new DataObject(DataObject.VisibilityOptions.Member, "4") },
                { KEY_RECIPE_MODE, new DataObject(DataObject.VisibilityOptions.Member, "0") },
            };
        }

        return lobbyData;
    }
}