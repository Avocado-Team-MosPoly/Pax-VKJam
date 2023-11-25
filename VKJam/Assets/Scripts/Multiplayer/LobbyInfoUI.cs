using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI playersCount;
    [SerializeField] private Button connectButton;
    [SerializeField] private GameObject RegimeMode;
    [SerializeField] private Sprite competitive;
    [SerializeField] private Sprite comand;

    private Lobby lobby;

    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        if (lobby.Data["IsTeamMode"].Value=="1")
        {
            RegimeMode.GetComponent<Image>().sprite = competitive;
        }
        else
        {
            RegimeMode.GetComponent<Image>().sprite = comand;
        }

        lobbyName.text = lobby.Name;
        playersCount.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        connectButton.onClick.AddListener(() => {
            LobbyManager.Instance.JoinLobby(lobby);
        });
    }
}