using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayersDataManager : NetworkBehaviour
{
    private struct NetworkTuple_PlayerData : INetworkSerializable, IEquatable<NetworkTuple_PlayerData>
    {
        public byte Id;
        public PlayerData PlayerData;

        public NetworkTuple_PlayerData(byte id, PlayerData playerData)
        {
            Id = id;
            PlayerData = playerData;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref PlayerData);
        }

        public bool Equals(NetworkTuple_PlayerData other)
        {
            return this.Id == other.Id
                && this.PlayerData.Equals(other.PlayerData);
        }
    }

    public event Action<ulong> PlayerConnected;
    public event Action<ulong> PlayerDisconnected;

    [SerializeField] private StoreSection avatarsAndFramesStorage;

    private Dictionary<ulong, PlayerData> _playersData = new();
    private NetworkList<NetworkTuple_PlayerData> playersDataList = new();

    public static PlayersDataManager Instance { get; private set; }

    public IReadOnlyDictionary<ulong, PlayerData> PlayersData => _playersData;
    public int PlayersCount => _playersData.Count;
    public StoreSection AvatarsAndFramesStorage => avatarsAndFramesStorage;

    public override void OnNetworkSpawn()
    {
        Logger.Instance.Log(this, "Spawned");

        Init();

        var avatarAndFrameIndexes = GetAvatarAndFrameIndexes();
        AddPlayerData(new PlayerData(avatarAndFrameIndexes.avatarIndex, avatarAndFrameIndexes.frameIndex));

        if (IsServer)
            RelayManager.Instance.OnClientDisconnect.AddListener(OnClientDisconnect);
        //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void Init()
    {
        if (Instance == null)
        {
            Instance = this;

            foreach (NetworkTuple_PlayerData tuple in playersDataList)
            {
                _playersData[tuple.Id] = tuple.PlayerData;
                Logger.Instance.Log(this, $"Player {tuple.Id} data added: {tuple.PlayerData})");
            }

            playersDataList.OnListChanged += OnPlayersDataListChanged;
        }
        else
        {
            Logger.Instance.LogError(this, $"Second {nameof(PlayersDataManager)} spawned");
            return;
        }
    }

    public void AddPlayerData(PlayerData playerData)
    {
        ulong localClientId = NetworkManager.LocalClientId;

        if (!_playersData.TryAdd(localClientId, playerData))
        {
            Logger.Instance.LogWarning(this, $"Player data already stored. Use {nameof(ChangePlayerData)} instead");
            return;
        }

        if (IsServer)
            playersDataList.Add(new NetworkTuple_PlayerData((byte)localClientId, playerData));
        else
            AddPlayerDataServerRpc(playerData, new ServerRpcParams());
    }

    public void ChangePlayerData(PlayerData playerData)
    {
        ulong localClientId = NetworkManager.LocalClientId;

        if (!_playersData.ContainsKey(localClientId))
        {
            Logger.Instance.LogWarning(this, $"{nameof(_playersData)} doesn't contain player data. First you need to call {nameof(AddPlayerData)}");
            return;
        }

        if (IsServer)
            playersDataList.Add(new NetworkTuple_PlayerData((byte)localClientId, playerData));
        else
            ChangePlayerDataServerRpc(playerData, new ServerRpcParams());
    }

    private (byte avatarIndex, byte frameIndex) GetAvatarAndFrameIndexes()
    {
        (byte avatarIndex, byte frameIndex) result = (7, 0);

        if (CustomController.Instance == null)
            return result;

        for (int i = 0; i < avatarsAndFramesStorage.products.Count; i++)
        {
            if (avatarsAndFramesStorage.products[i].Data.Type == ItemType.Avatars)
            {
                if (CustomController.Instance.Custom[(int)ItemType.Avatars].Data.productName == avatarsAndFramesStorage.products[i].Data.productName)
                {
                    result.avatarIndex = (byte)i;
                    break;
                }
            }
        }

        for (int i = 0; i < avatarsAndFramesStorage.products.Count; i++)
        {
            if (avatarsAndFramesStorage.products[i].Data.Type == ItemType.AvatarFrame)
            {
                if (CustomController.Instance.Custom[(int)ItemType.AvatarFrame].Data.productName == avatarsAndFramesStorage.products[i].Data.productName)
                {
                    result.frameIndex = (byte)i;
                    break;
                }
            }
        }

        return result;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerDataServerRpc(PlayerData playerData, ServerRpcParams rpcParams)
    {
        playersDataList.Add(new NetworkTuple_PlayerData((byte)rpcParams.Receive.SenderClientId, playerData));
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerDataServerRpc(PlayerData playerData, ServerRpcParams rpcParams)
    {
        int index = 0;
        for (; index < playersDataList.Count; index++)
            if (playersDataList[index].Id == rpcParams.Receive.SenderClientId)
                break;

        if (index > playersDataList.Count)
        {
            Logger.Instance.LogWarning(this, $"{nameof(playersDataList)} doesn't contain player {rpcParams.Receive.SenderClientId} data");
            return;
        }

        _playersData[rpcParams.Receive.SenderClientId] = playerData;
        playersDataList[index] = new NetworkTuple_PlayerData((byte)rpcParams.Receive.SenderClientId, playerData);
    }

    private void OnPlayersDataListChanged(NetworkListEvent<NetworkTuple_PlayerData> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<NetworkTuple_PlayerData>.EventType.RemoveAt ||
            changeEvent.Type == NetworkListEvent<NetworkTuple_PlayerData>.EventType.Remove)
        {
            Logger.Instance.Log(this, $"change event id {changeEvent.PreviousValue.Id} {changeEvent.Value.Id}");
            _playersData.Remove(changeEvent.Value.Id);
            PlayerDisconnected?.Invoke(changeEvent.Value.Id);
            Logger.Instance.Log(this, $"Player {changeEvent.Value.Id} data removed");
        }
        else if (changeEvent.Type == NetworkListEvent<NetworkTuple_PlayerData>.EventType.Add)
        {
            _playersData[changeEvent.Value.Id] = changeEvent.Value.PlayerData;
            PlayerConnected?.Invoke(changeEvent.Value.Id);
            Logger.Instance.Log(this, $"Player {changeEvent.Value.Id} data added: {changeEvent.Value.PlayerData}");
        }
        else if (changeEvent.Type == NetworkListEvent<NetworkTuple_PlayerData>.EventType.Value)
        {
            _playersData[changeEvent.Value.Id] = changeEvent.Value.PlayerData;
            Logger.Instance.Log(this, $"Player {changeEvent.Value.Id} data updated: {changeEvent.Value.PlayerData}");
        }
        else
        {
            Logger.Instance.LogError(this, $"Unexpected change event type: {changeEvent.Type}");
        }
    }

    // server
    private void OnClientDisconnect(ulong clientId)
    {
        if (!IsServer)
            return;

        for (int i = 0; i < playersDataList.Count; i++)
        {
            if (playersDataList[i].Id == clientId)
                playersDataList.RemoveAt(i);
        }
    }
}