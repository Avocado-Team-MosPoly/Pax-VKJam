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
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

#if UNITY_EDITOR
            Debug.Log("EDITOR");
            
            //UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            //unityTransport.UseWebSockets = false;

            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData
            //(
            //    allocation.RelayServer.IpV4,
            //    (ushort)allocation.RelayServer.Port,
            //    allocation.AllocationIdBytes,
            //    allocation.Key,
            //    allocation.ConnectionData
            //);
#else
            Debug.Log("BUILD");
            RelayServerData relayServerData = new RelayServerData(allocation, "wss");

            UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

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

#if UNITY_EDITOR
            //Debug.Log("EDITOR");

            //UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            //unityTransport.UseWebSockets = false;

            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData
            //(
            //    joinAllocation.RelayServer.IpV4,
            //    (ushort)joinAllocation.RelayServer.Port,
            //    joinAllocation.AllocationIdBytes,
            //    joinAllocation.Key,
            //    joinAllocation.ConnectionData,
            //    joinAllocation.HostConnectionData
            //);
#else
            Debug.Log("BUILD");
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "wss");

            UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

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