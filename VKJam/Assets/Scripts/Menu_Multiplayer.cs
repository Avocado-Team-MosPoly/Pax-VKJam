using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Menu_Multiplayer : MonoBehaviour
{
    private struct RoomSettings
    {
        int maxPlayerAmount;
    }

    [SerializeField] private string lobbySceneName;

    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    private void Start()
    {
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
    }

    private void CreateRoom()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) => { Debug.Log($"Client {clientId} connected"); };
        NetworkManager.Singleton.StartHost();

        SceneLoader.ServerLoad(lobbySceneName);
    }

    private void JoinRoom()
    {
        NetworkManager.Singleton.StartClient();

        //SceneLoader.Load(lobbySceneName);
    }
}