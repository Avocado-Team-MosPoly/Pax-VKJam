using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using UnityEngine.Events;
using System;

public class RelayManager : MonoBehaviour
{
    [HideInInspector] public UnityEvent<ulong> OnClientConnected;
    [HideInInspector] public UnityEvent<ulong> OnClientDisconnect;

    public string LobbySceneName => lobbySceneName;

    [SerializeField] private string lobbySceneName;

    [Header("Prefabs")]
    [SerializeField] private PlayersDataManager playersDataManagerPrefab;

    public static RelayManager Instance { get; private set; }

    /*
        Allowed regions:
            Works in VK (can cause a few connection failed errors):
                europe-north1
                us-central1
                asia-northeast1
                asia-south1
                asia-northeast3
                australia-southeast1

            Works in VK (sometimes connection failed fully):
                asia-southeast1
                asia-southeast2

            Doesn't works in VK:
                europe-central2
                europe-west4
                europe-west2
                us-east4 - 
                northamerica-northeast1
                us-west2
                southamerica-east1
    */
    private string connectionRegion = "europe-north1";

    public void ChangeRegion(string region)
    {
        connectionRegion = region;
    }

    private void Awake()
    {
#if UNITY_EDITOR
#if UNITY_WEBGL
        Logger.Instance.LogError(this, new PlatformNotSupportedException("Multiplyer doesn't work in editor on \"WebGL\" platform. You should change platform to \"Windows, Mac, Linux\""));
        UnityEditor.EditorApplication.isPlaying = false;
#endif
#endif
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance.gameObject);

            NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) =>
            {
                OnClientConnected?.Invoke(clientId);
            };
            NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) =>
            {
                OnClientDisconnect?.Invoke(clientId);
            };
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4, connectionRegion);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

#if UNITY_EDITOR
            Logger.Instance.Log(this, "EDITOR");

            unityTransport.SetRelayServerData
            (
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
#else
            Logger.Instance.Log(this, "BUILD");
#if UNITY_STANDALONE_WIN
            Logger.Instance.Log(this, "Windows");

            unityTransport.SetRelayServerData
            (
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
#endif
#if UNITY_WEBGL
            Logger.Instance.Log(this, "WebGL");

            RelayServerData relayServerData = new RelayServerData(allocation, "wss");

            unityTransport.UseWebSockets = true;
            unityTransport.SetRelayServerData(relayServerData);
#endif
#endif
            NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) =>
            {
                Logger.Instance.Log(this, $"Client {clientId} connected");
                //SpawnPlayersDataManagerWithOwnership(clientId);
            };
            NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) =>
            {
                Logger.Instance.Log(this, NetworkManager.Singleton.DisconnectReason);
            };

            NetworkManager.Singleton.OnServerStarted += () =>
            {
                PlayersDataManager pdmInstance = Instantiate(playersDataManagerPrefab);
                pdmInstance.NetworkObject.Spawn();

                SceneLoader.ServerLoad(lobbySceneName);
            };



            NetworkManager.Singleton.OnClientStopped += (bool isHost) =>
            {
                //if (PlayersDataManager.Instance != null && PlayersDataManager.Instance.NetworkObject != null)
                //    PlayersDataManager.Instance.NetworkObject.Despawn();

                SceneLoader.Load("Menu");
            };

            NetworkManager.Singleton.OnClientStarted += () =>
            {
                Logger.Instance.Log(this, $"Client Started on server\nCurrent Lobby: {LobbyManager.Instance.CurrentLobby.Name}\nLobby Player Id: {LobbyManager.Instance.LobbyPlayerId}");
            };
            NetworkManager.Singleton.StartHost();

            Logger.Instance.Log(this, "You created relay with code: " + joinCode);
            return joinCode;
        }
        catch (RelayServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
            NotificationSystem.Instance.SendLocal("Connection error: Can't connect to Unity Relay servers.");

            LobbyManager.Instance.Disconnect();

            return "0";
        }
    }

    public async Task JoinRelay(string joinCode)
    {
        try
        {
            Logger.Instance.Log(this, joinCode);

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

#if UNITY_EDITOR
            Logger.Instance.Log(this, "EDITOR");

            unityTransport.SetRelayServerData
            (
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
#else
            Logger.Instance.Log(this, "BUILD");
#if UNITY_STANDALONE_WIN
            Logger.Instance.Log(this, "Windows");
            
            unityTransport.SetRelayServerData
            (
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
#endif
#if UNITY_WEBGL
            Logger.Instance.Log(this, "WebGL");

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "wss");

            unityTransport.UseWebSockets = true;
            unityTransport.SetRelayServerData(relayServerData);
#endif
#endif
            NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) => Debug.Log(NetworkManager.Singleton.DisconnectReason);

            NetworkManager.Singleton.OnClientStarted += /*async*/ () =>
            {
                Logger.Instance.Log(this, "Client Started");
            };
            NetworkManager.Singleton.OnClientStopped += async (bool isHost) =>
            {
                Disconnect();
                await LobbyManager.Instance.LeaveLobbyAsync();
                SceneLoader.Load("Menu");
            };

            NetworkManager.Singleton.StartClient();

            Logger.Instance.Log(this, "You joined relay with code: " +  joinCode);
        }
        catch (RelayServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
            NotificationSystem.Instance.SendLocal("Connection error: Can't connect to Unity Relay servers.");

            LobbyManager.Instance.Disconnect();
        }
    }

    public void DisconnectPlayer(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        NetworkManager.Singleton.DisconnectClient(clientId);
        //LobbyManager.Instance.DisconnectPlayer(clientId);
    }

    public void Disconnect()
    {
        try
        {
            NetworkManager.Singleton.Shutdown();
            //while (NetworkManager.Singleton.ShutdownInProgress) { }
            Logger.Instance.Log(this, "Disconnected from server");
        }
        catch (Exception ex)
        {
            Logger.Instance.LogError(this, ex);
        }
    }

    public void ReturnToLobby()
    {
        SceneLoader.ServerLoad(lobbySceneName);
    }
}