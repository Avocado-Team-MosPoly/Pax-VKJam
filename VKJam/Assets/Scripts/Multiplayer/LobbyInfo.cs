using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI recipeMode;
    [SerializeField] private TextMeshProUGUI playersCount;
    [SerializeField] private TextMeshProUGUI gameMode;
    [SerializeField] private TextMeshProUGUI roundCount;

    [SerializeField] private Button connectButton;
    [SerializeField] private Button closeButton;

    private Lobby lobby;

    [HideInInspector] public UnityEvent<Lobby> OnLobbyChoosed;

    public static LobbyInfo Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        connectButton.onClick.AddListener(() => {
            LobbyManager.Instance.JoinLobbyAsync(lobby);
        });

        closeButton.onClick.AddListener(() => {
            gameObject.SetActive(false);
        });

        OnLobbyChoosed.AddListener(
            (Lobby lobby) => this.lobby = lobby
        );

        gameObject.SetActive(false);
    }

    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        OnLobbyChoosed?.Invoke(lobby);
        
        lobbyName.text = lobby.Name;
        playersCount.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        gameMode.text = lobby.Data[LobbyManager.Instance.KEY_TEAM_MODE].Value == "True" ? "Командный" : "Соревновательный";
        roundCount.text = lobby.Data[LobbyManager.Instance.KEY_ROUND_AMOUNT].Value;
        recipeMode.text = lobby.Data[LobbyManager.Instance.KEY_RECIPE_MODE].Value == "0" ? "Стандарные" : "Случайные";
        
        gameObject.SetActive(true);
    }
}