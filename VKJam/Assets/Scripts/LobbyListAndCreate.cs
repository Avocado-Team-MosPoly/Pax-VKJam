using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListAndCreate : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;

    [SerializeField] private TMP_Dropdown filterRegim;
    [SerializeField] private Button[] listLobbiesButtons;

    [Header("Lobbies list")]
    [SerializeField] private RectTransform lobbyListContainer;
    [SerializeField] private GameObject lobbyInfoTemplate;

    private void Start()
    {
        LobbyManager.Instance.OnLobbyListed.AddListener(UpdateLobbyList);
        createLobbyButton.onClick.AddListener(LobbyManager.Instance.CreateLobby);
        filterRegim.onValueChanged.AddListener(UpdateModeFilter);

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

    private void UpdateModeFilter(int value)
    {
        LobbyManager.Instance.IsTeamModeFilter = value;
    }
}