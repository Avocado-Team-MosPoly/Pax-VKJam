using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI playersCount;

    private Lobby lobby;

    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        
        lobbyName.text = lobby.Name;
        playersCount.text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        GetComponent<Button>().onClick.AddListener(
            () => LobbyInfo.Instance.SetLobby(this.lobby)
        );
    }
}