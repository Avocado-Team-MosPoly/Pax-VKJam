using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class Menu_Multiplayer : MonoBehaviour
{
    public struct RoomSettings
    {
        private string name;
        private int maxPlayerAmount;
        private int maxRoundAmount;
        private bool isTeamMode;

        public string Name => name;
        public int MaxPlayerAmount => maxPlayerAmount;
        public int MaxRoundAmount => maxRoundAmount;
        public bool IsTeamMode => isTeamMode;

        public RoomSettings(string name, int maxPlayerAmount, int maxRoundAmount, bool isTeamMode)
        {
            this.name = name;
            this.maxPlayerAmount = maxPlayerAmount;
            this.maxRoundAmount = maxRoundAmount;
            this.isTeamMode = isTeamMode;
        }
    }

    [SerializeField] private string lobbySceneName;

    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    [SerializeField] private TMP_Dropdown deckDropdown;
    [SerializeField] private TMP_Dropdown roundDropdown;

    public static RoomSettings roomSettings { get; private set; }

    private void Start()
    {
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
    }

    private void CreateRoom()
    {
        // пока что захардкожено, тк реализован только комендный режим (неважно количество игроков и раундов)
        roomSettings = new("Main", 2, 4, true);

        NetworkManager.Singleton.OnServerStarted += () => SceneLoader.ServerLoad(lobbySceneName);
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) => { Debug.Log($"Client {clientId} connected"); };
        NetworkManager.Singleton.StartHost();
    }

    private void JoinRoom()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void ChangeMaxPlayersAmount(int value)
    {
        
    }

    public void SelectGameMode(bool isTeamMode)
    {
        
    }

    public void SelectRecipeMode(bool isStandart)
    {
        
    }

    public void SelectDeck()
    {
        //int selectedDeckIndex = deckDropdown.value;
    }

    public void SelectRoundCount()
    {
        //int selectedRoundIndex = roundDropdown.value;
    }
}