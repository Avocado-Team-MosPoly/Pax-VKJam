using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using UnityEngine.Events;
using System;
using System.Collections;

public class RelayManager : MonoBehaviour
{
    [HideInInspector] public UnityEvent<ulong> OnClientConnected;
    [HideInInspector] public UnityEvent<ulong> OnClientDisconnect;

    public string LobbySceneName => lobbySceneName;
    public bool IsServer => NetworkManager.Singleton.IsServer;
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

    public IEnumerator Init()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance.gameObject);
        }
        else
        {
            Destroy(this);
            yield break;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientStopped += OnClientStopped;

        Logger.Instance.Log(this, "Initialized");
    }

    private bool PlatformNotSupportedException()
    {
#if UNITY_EDITOR
#if UNITY_WEBGL
        Logger.Instance.LogError(this, new PlatformNotSupportedException("Multiplyer doesn't work in editor on \"WebGL\" platform. You should change platform to \"Windows, Mac, Linux\""));
        NotificationSystem.Instance.SendLocal("Platform not supporteed");
        UnityEditor.EditorApplication.isPaused = true;
        return true;
#endif
#endif
#pragma warning disable CS0162
        return false;
#pragma warning restore CS0162
    }

    public async Task<string> CreateRelay()
    {
        if (PlatformNotSupportedException())
        {
            return "0";
        }

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
#elif UNITY_WEBGL
            Logger.Instance.Log(this, "WebGL");

            RelayServerData relayServerData = new RelayServerData(allocation, "wss");

            unityTransport.UseWebSockets = true;
            unityTransport.SetRelayServerData(relayServerData);

#elif UNITY_ANDROID
            NotificationSystem.Instance.SendLocal("UNITY_ANDROID_1");

            unityTransport.SetRelayServerData
            (
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

#else
            Logger.Instance.LogError(this, "Unsupported Platform. Supprted Platforms: UNITY_WEBGL, UNITY_STANDALONE_WIN");
            return "0";
#endif
#endif
            unityTransport.SetRelayServerData
            (
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallbackServer;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallbackServer;
            NetworkManager.Singleton.OnServerStarted += OnServerStartedServer;
            NetworkManager.Singleton.OnClientStarted += OnClientStartedServer;
            NetworkManager.Singleton.OnClientStopped += OnClientStoppedServer;

            NetworkManager.Singleton.StartHost();

            Logger.Instance.Log(this, "You created relay with code: " + joinCode);
            return joinCode;
        }
        catch (RelayServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
            NotificationSystem.Instance.SendLocal("Ошибка соединения: Не получилось подключиться к серверам Unity Relay");

            //await LobbyManager.Instance.DisconnectAsync();

            return "0";
        }
    }

    public async Task<bool> JoinRelay(string joinCode)
    {
        if (PlatformNotSupportedException())
        {
            return false;
        }

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
#if UNITY_ANDROID
            Logger.Instance.Log(this, "Windows");
            
            NotificationSystem.Instance.SendLocal("UNITY_ANDROID");
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
#endif
            NotificationSystem.Instance.SendLocal("Android");

            unityTransport.SetRelayServerData
            (
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallbackClient;
            NetworkManager.Singleton.OnClientStarted += OnClientStartedClient;
            NetworkManager.Singleton.OnClientStopped += OnClientStoppedClient;

            NetworkManager.Singleton.StartClient();

            Logger.Instance.Log(this, "You joined relay with code: " +  joinCode);

            return true;
        }
        catch (RelayServiceException ex)
        {
            Logger.Instance.LogError(this, ex);
            NotificationSystem.Instance.SendLocal("Ошибка соединения: Не получилось подключиться к серверам Unity Relay");

            return false;
        }
    }

    #region Events

    // common
    private void OnClientConnectedCallback(ulong clientId)
    {
        OnClientConnected?.Invoke(clientId);

        Logger.Instance.Log(this, $"Client {clientId} connected");
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        OnClientDisconnect?.Invoke(clientId);

        Logger.Instance.Log(this, $"Client {clientId} disconnected");
    }

    private void OnClientStopped(bool isHost)
    {
        if (isHost)
        {
            //Logger.Instance.Log(this, "Host unsub");
            // server
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallbackServer;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallbackServer;
            NetworkManager.Singleton.OnClientStarted -= OnServerStartedServer;
            NetworkManager.Singleton.OnClientStarted -= OnClientStartedServer;
            NetworkManager.Singleton.OnClientStopped -= OnClientStoppedServer;
        }
        else
        {
            //Logger.Instance.Log(this, "Client unsub");
            // client
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallbackClient;
            NetworkManager.Singleton.OnClientStarted -= OnClientStartedClient;
            NetworkManager.Singleton.OnClientStopped -= OnClientStoppedClient;
        }

        //await LobbyManager.Instance.DisconnectAsync();
        //Logger.Instance.Log(this, "Disconnected");
        Logger.Instance.Log(this, "Client Stopped");
    }

    // server
    private void OnClientConnectedCallbackServer(ulong clientId)
    {
        //Logger.Instance.Log(this, "OnClientConnectedCallbackServer");
        //SpawnPlayersDataManagerWithOwnership(clientId);
    }

    private void OnClientDisconnectCallbackServer(ulong clientId)
    {
        //Logger.Instance.Log(this, "OnClientDisconnectCallbackServer");
        //Logger.Instance.Log(this, NetworkManager.Singleton.DisconnectReason);
    }

    private void OnServerStartedServer()
    {
        //Logger.Instance.Log(this, "OnServerStartedServer");

        PlayersDataManager pdmInstance = Instantiate(playersDataManagerPrefab);
        pdmInstance.NetworkObject.Spawn();

        SceneLoader.ServerLoad(lobbySceneName);
    }

    private void OnClientStartedServer()
    {
        //Logger.Instance.Log(this, "OnClientStartedServer");

        Logger.Instance.Log(this, $"Client Started on server\nCurrent Lobby: {LobbyManager.Instance.CurrentLobby.Name}\nLobby Player Id: {LobbyManager.Instance.PlayerId}");
    }

    private void OnClientStoppedServer(bool isHost)
    {
        //Logger.Instance.Log(this, "OnClientStoppedServer");

        //if (PlayersDataManager.Instance != null && PlayersDataManager.Instance.NetworkObject != null)
        //    PlayersDataManager.Instance.NetworkObject.Despawn();

        SceneLoader.Load("Menu");
    }

    // client
    private void OnClientDisconnectCallbackClient(ulong clientId)
    {
        //Logger.Instance.Log(this, "OnClientDisconnectCallbackClient");

        //Logger.Instance.Log(this, NetworkManager.Singleton.DisconnectReason);
    }

    private void OnClientStartedClient()
    {
        //Logger.Instance.Log(this, "OnClientStartedClient");

        Logger.Instance.Log(this, "Client Started");
    }

    private void OnClientStoppedClient(bool isHost)
    {
        //Logger.Instance.Log(this, "OnClientStoppedClient");

        //Disconnect();
        //await LobbyManager.Instance.DisconnectAsync();
        SceneLoader.Load("Menu");
    }

    #endregion

    public void ChangeRegion(string region)
    {
        connectionRegion = region;
    }

    public void DisconnectPlayer(ulong clientId)
    {
        if (!IsServer)
            return;

        NetworkManager.Singleton.DisconnectClient(clientId);
        Logger.Instance.Log(this, clientId + " disconnected");
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