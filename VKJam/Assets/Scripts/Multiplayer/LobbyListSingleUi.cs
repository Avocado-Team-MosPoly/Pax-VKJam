using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyNameTextFullSeting;
    [SerializeField] private TextMeshProUGUI receipeMode;
    [SerializeField] private TextMeshProUGUI playerCount;
    [SerializeField] private TextMeshProUGUI playerCountFullSeting;
    [SerializeField] private TextMeshProUGUI gameMode;
    [SerializeField] private TextMeshProUGUI roundCount;

    [SerializeField] private GameObject connectButton;
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject fullSetting;

    private Lobby lobby;

    public void UpdateLobby(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
        playerCount.text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        GetComponent<Button>().onClick.AddListener(() => {
            UpdateFullSeting(lobby);
        });
    }
    public void UpdateFullSeting(Lobby lobby)
    {
        fullSetting.SetActive(true);
        lobbyNameTextFullSeting.text = lobby.Name;
        playerCountFullSeting.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        gameMode.text = lobby.Data[LobbyManager.Instance.KEY_TEAM_MODE].Value;
        roundCount.text = lobby.Data[LobbyManager.Instance.KEY_ROUND_AMOUNT].Value;
        receipeMode.text = lobby.Data[LobbyManager.Instance.KEY_RECIPE_MODE].Value;

        connectButton.GetComponent<Button>().onClick.AddListener(() => {
            LobbyManager.Instance.JoinLobby(this.lobby);
        });
        closeButton.GetComponent<Button>().onClick.AddListener(() => {
            fullSetting.SetActive(false);
        });
    }


}
