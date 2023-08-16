using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManagerUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button[] listLobbiesButtons;

    [SerializeField] private RectTransform lobbyListContainer;
    [SerializeField] private GameObject lobbyInfoTemplate;

    [SerializeField] private RectTransform playerListContainer;
    [SerializeField] private GameObject playerInfoPrefab;

    private void Start()
    {
        createLobbyButton.onClick.AddListener(LobbyManager.Instance.CreateLobby);
        
        foreach (var button in listLobbiesButtons)
            button.onClick.AddListener(LobbyManager.Instance.ListLobbies);

        LobbyManager.Instance.OnLobbyListed.AddListener(UpdateLobbyList);
        LobbyManager.Instance.OnPlayerListed.AddListener(UpdatePlayerList);
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
        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);

        foreach (Player player in players)
        {
            GameObject playerInfoInstance = Instantiate(playerInfoPrefab, playerListContainer);
            playerInfoInstance.SetActive(true);

            PlayerInfoUI playerInfoUI = playerInfoInstance.GetComponent<PlayerInfoUI>();
            playerInfoUI.UpdatePlayer(player);
        }
    }
}