using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using Unity.Services.Core;
using Unity.Services;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;

public class Menu_Multiplayer : MonoBehaviour
{
    public enum RecipeMode
    {
        Standard,
        Random
    }

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

    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private string joinCode;

    private RoomSettings roomSettings;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => Debug.Log("Signed In. Your Id is " + AuthenticationService.Instance.PlayerId);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
        joinCodeInputField.onValueChanged.AddListener((string value) => joinCode = value);
    }

    private async void CreateRoom()
    {
        // пока что захардкожено, тк реализован только комендный режим (неважно количество игроков и раундов)
        //roomSettings = new
        //(
        //    name:"Main",
        //    maxPlayerAmount:2,
        //    isTeamMode:true,
        //    maxRoundAmount:4,
        //    recipeMode:RecipeMode.Standard
        //);
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            if (!Log(joinCode))
                Debug.Log(joinCode);
            Debug.Log(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData
            (
                allocation.RelayServer.IpV4,
                (ushort) allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) => { Debug.Log($"Client {clientId} connected"); };
            NetworkManager.Singleton.OnServerStarted += () => SceneLoader.ServerLoad(lobbySceneName);
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException ex)
        {
            if (!Log(ex))
                throw;
        }
    }

    private async void JoinRoom()
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData
            (
                joinAllocation.RelayServer.IpV4,
                (ushort) joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
        }        
        catch (RelayServiceException ex)
        {
            if (!Log(ex))
                throw;
        }
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