using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI playersCount;
    [SerializeField] private Button connectButton;

    private Lobby lobby;

    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        
        lobbyName.text = lobby.Name;
        playersCount.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        connectButton.onClick.AddListener(() => {
            LobbyManager.Instance.JoinLobby(lobby);
        });
    }
}