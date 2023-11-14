using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class LobbyManagerUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button[] listLobbiesButtons;
    [SerializeField] private Button listPlayersButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button updatePlayerList;

    [SerializeField] private RectTransform lobbyListContainer;
    [SerializeField] private GameObject lobbyInfoTemplate;
    [SerializeField] private GameObject ready;
    [SerializeField] private List<GameObject> playerGameObjectList;
    [SerializeField] private List<GameObject> playerReady;

    [SerializeField] private RectTransform playerListContainer;
    [SerializeField] private GameObject playerInfoPrefab;
    [SerializeField] private List<bool> allPlayerReady;
    private List<Player> players;


    private void Start()
    {
        createLobbyButton?.onClick.AddListener(LobbyManager.Instance.CreateLobby);
        leaveLobbyButton?.onClick.AddListener(LeaveLobby);
        updatePlayerList?.onClick.AddListener(LobbyManager.Instance.ListPlayers);
        ready?.GetComponent<Button>().onClick.AddListener(ChangeReady);

        if (startGameButton)
        {
            if (NetworkManager.Singleton.IsHost)
                startGameButton.onClick.AddListener(() => SceneLoader.ServerLoad("Map_New"));
            else
                startGameButton.gameObject.SetActive(false);
        }

        foreach (var button in listLobbiesButtons)
            button.onClick.AddListener(LobbyManager.Instance.ListLobbies);

        if (lobbyListContainer && lobbyInfoTemplate)
            LobbyManager.Instance.OnLobbyListed.AddListener(UpdateLobbyList);

        if (playerListContainer && playerInfoPrefab)
        {
            LobbyManager.Instance.OnPlayerListed.AddListener(UpdatePlayerList);
            LobbyManager.Instance.ListPlayers();
        }
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyListContainer)
            Destroy(child.gameObject);

        foreach (Lobby lobby in lobbyList)
        {
            GameObject lobbySingleTransform = Instantiate(lobbyInfoTemplate, lobbyListContainer);
            lobbySingleTransform.SetActive(true);

            LobbyInfoUI lobbyInfoUI = lobbySingleTransform.GetComponent<LobbyInfoUI>();
            lobbyInfoUI.SetLobby(lobby);
        }
    }

    private void UpdatePlayerList(List<Player> players)
    {
        UpdatePlayerListServerRpc(players);
    }
    private int GetplayerNumber()
    {
        int i = 0;
        for (; i < players.Count; i++)
        {
            if (players[i].Id == AuthenticationService.Instance.PlayerId)
            {
                
                break;
            }
        }
        return i;
    }
    public void ChangeReady()
    {
        UpdatePlayerReadyServerRpc(GetplayerNumber());
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerReadyServerRpc(int player)
    {
        allPlayerReady[player]=!allPlayerReady[player];
        UpdatePlayerReadyClientRpc(allPlayerReady);
    }
    [ClientRpc]
    private void UpdatePlayerReadyClientRpc(List<bool> players)
    {
        allPlayerReady = players;
        for (int i = 0; i < playerReady.Count; i++)
        {
            playerReady[i].SetActive(allPlayerReady[i]);
        }
        if(NetworkManager.Singleton.IsServer)
        {
            bool allReady=true;
            foreach (bool ready in allPlayerReady)
            {
                if (!ready)
                    allReady = false;
            }
            if (allReady)
            {
                
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerListServerRpc(List<Player> players)
    {
        UpdatePlayerListClientRpc(players);
    }
    [ClientRpc]
    private void UpdatePlayerListClientRpc(List<Player> players)
    {
        this.players=players;
        foreach (GameObject player in playerGameObjectList)
        {
            player.SetActive(false);
        }
        for (int i = 0; i < players.Count; i++)
        {
            playerGameObjectList[i].SetActive(true);
        }
    }
    private void LeaveLobby()
    {
        LobbyManager.Instance.LeaveLobby();
    }
}