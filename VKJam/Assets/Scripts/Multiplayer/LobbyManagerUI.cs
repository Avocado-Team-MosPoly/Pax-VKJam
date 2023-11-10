using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private List<GameObject> playerList;

    [SerializeField] private RectTransform playerListContainer;
    [SerializeField] private GameObject playerInfoPrefab;

    private void Start()
    {
        createLobbyButton?.onClick.AddListener(LobbyManager.Instance.CreateLobby);
        leaveLobbyButton?.onClick.AddListener(LeaveLobby);
        updatePlayerList?.onClick.AddListener(LobbyManager.Instance.ListPlayers);

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
        foreach (GameObject player in playerList)
        {
            player.SetActive(false);
        }
        for (int i =0; i< players.Count; i++)
        {
            playerList[i].SetActive(true);

        }
    }

    private void LeaveLobby()
    {
        LobbyManager.Instance.LeaveLobby();
    }
}