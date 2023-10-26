using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;

public class RelayManager : MonoBehaviour
{
    public struct RoomSettings
    {
        public string Name;
        public int MaxPlayers;
        public bool IsTeamMode;

        public int RoundAmount;
        public RecipeMode RecipeMode;

        public RoomSettings(string name)
        {
            Name = name;
            MaxPlayers = 2;
            IsTeamMode = true;

            RoundAmount = 4;
            RecipeMode = RecipeMode.Standard;
        }

        public RoomSettings(string name, int maxPlayerAmount, bool isTeamMode, int maxRoundAmount, RecipeMode recipeMode)
        {
            Name = name;
            MaxPlayers = maxPlayerAmount;
            IsTeamMode = isTeamMode;

            RoundAmount = maxRoundAmount;
            RecipeMode = recipeMode;
        }
    }

    [SerializeField] private string lobbySceneName;

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance.gameObject);
        }
        else
        {
            Destroy(this);
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
            Debug.Log("EDITOR");

            unityTransport.SetRelayServerData
            (
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

#else
            Debug.Log("BUILD");
            
            RelayServerData relayServerData = new RelayServerData(allocation, "wss");

            unityTransport.UseWebSockets = true;
            unityTransport.SetRelayServerData(relayServerData);
#endif

            NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) => { Debug.Log($"Client {clientId} connected"); };
            NetworkManager.Singleton.OnServerStarted += () => SceneLoader.ServerLoad(lobbySceneName);
            NetworkManager.Singleton.StartHost();

            Log("You created relay with code: " + joinCode);

            return joinCode;
        }
        catch (RelayServiceException ex)
        {
            if (!Log(ex))
                throw;

            return "0";
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Log(joinCode);

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

#if UNITY_EDITOR
            Debug.Log("EDITOR");

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
            Debug.Log("BUILD");
            
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "wss");

            unityTransport.UseWebSockets = true;
            unityTransport.SetRelayServerData(relayServerData);
#endif

            NetworkManager.Singleton.OnClientStarted += () => Logger.Instance.Log("Client Started");
            NetworkManager.Singleton.StartClient();

            Log("You joined relay with code: " +  joinCode);
        }
        catch (RelayServiceException ex)
        {
            if (!Log(ex))
                throw;
        }
    }

    public void ReturnToLobby()
    {
        SceneLoader.ServerLoad(lobbySceneName);
    }

    private bool Log(object message)
    {
        if (Logger.Instance)
        {
            Logger.Instance.Log(message);
            return true;
        }

        return false;
    }
}