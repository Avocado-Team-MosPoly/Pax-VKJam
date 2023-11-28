using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreationManagerUi : MonoBehaviour
{
    [SerializeField] private RectTransform lobbyListContainer;
    [SerializeField] private GameObject lobbyInfoTemplate;
    [SerializeField] private Button[] listLobbiesButtons;
    [SerializeField] private Button createLobbyButton;
    // Start is called before the first frame update
    void Start()
    {
        LobbyManager.Instance.OnLobbyListed.AddListener(UpdateLobbyList);
        createLobbyButton?.onClick.AddListener(LobbyManager.Instance.CreateLobby);
        foreach (var button in listLobbiesButtons)
            button.onClick.AddListener(LobbyManager.Instance.ListLobbies);
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
}
