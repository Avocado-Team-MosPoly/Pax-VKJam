using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    [SerializeField] private string hostScene;
    [SerializeField] private string clientScene;

    private void Start()
    {
        if (startHostButton)
            startHostButton.onClick.AddListener(StartHost);
        if (startClientButton)
            startClientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        CustomNetworkManager.Singleton.OnServerStarted += () => { Debug.Log("Server started"); };
        CustomNetworkManager.Singleton.OnClientConnectedCallback += (ulong id) => { Debug.Log($"Client {id} connected");  };
        CustomNetworkManager.Singleton.OnClientDisconnectCallback += (ulong id) => { Debug.Log($"Client {id} disconnect"); };
        
        CustomNetworkManager.Singleton.StartHost();

        SceneLoader.Load(hostScene);
    }

    private void StartClient()
    {
        CustomNetworkManager.Singleton.StartClient();

        SceneLoader.Load(clientScene);
    }
}