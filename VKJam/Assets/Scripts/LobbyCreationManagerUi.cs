using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreationManagerUi : MonoBehaviour
{
    [SerializeField] private RectTransform lobbyListContainer;
    [SerializeField] private GameObject lobbyInfoTemplate;
    [SerializeField] private Button[] listLobbiesButtons;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private TMP_Dropdown filterRegim;
    [SerializeField] private TMP_Dropdown filterMaxPlayer;

    private void Start()
    {
        LobbyManager.Instance.OnLobbyListed.AddListener(UpdateLobbyList);
        createLobbyButton?.onClick.AddListener(LobbyManager.Instance.CreateLobby);
        filterRegim?.onValueChanged.AddListener(UpdateRgimFiltr);
        filterMaxPlayer?.onValueChanged.AddListener(UpdatePlayerNumber);

        foreach (var button in listLobbiesButtons)
            button.onClick.AddListener(LobbyManager.Instance.ListLobbiesWithFilter);
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

    private void UpdateRgimFiltr(int value)
    {
        if (value == 0)
        {
            LobbyManager.Instance.IsTeamMode = "True";
        } 
        else if (value == 1)
        {
            LobbyManager.Instance.IsTeamMode = "False";
        }
    }
    private void UpdatePlayerNumber(int value)
    {
        LobbyManager.Instance.PlayerNumber = value;
    }
}
