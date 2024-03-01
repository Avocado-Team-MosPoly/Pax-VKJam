using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayersDataManager : NetworkBehaviour
{
    public static PlayersDataManager Instance { get; private set; }

    public IReadOnlyDictionary<ulong, PlayerData> PlayerDatas => playerDatas;

    public StoreSection AvatarsAndFramesStorage => avatarsAndFramesStorage;

    [SerializeField] private StoreSection avatarsAndFramesStorage;

    private Dictionary<ulong, PlayerData> playerDatas = new();
    private NetworkList<NetworkTuple_PlayerData> playerDatasList = new();

    private void Init()
    {
        if (Instance == null)
        {
            Instance = this;

            foreach (NetworkTuple_PlayerData tuple in playerDatasList)
            {
                playerDatas[tuple.Id] = tuple.PlayerData;
                Logger.Instance.Log(this, $"Player {tuple.Id} data added: {tuple.PlayerData})");
            }

            playerDatasList.OnListChanged += PlayerDatasList_OnListChanged;
        }
        else
        {
            Logger.Instance.LogError(this, $"Second {nameof(PlayersDataManager)} spawned");
            return;
        }
    }

    private void PlayerDatasList_OnListChanged(NetworkListEvent<NetworkTuple_PlayerData> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<NetworkTuple_PlayerData>.EventType.RemoveAt ||
            changeEvent.Type == NetworkListEvent<NetworkTuple_PlayerData>.EventType.Remove)
        {
            Logger.Instance.Log(this, $"change event id {changeEvent.PreviousValue.Id} {changeEvent.Value.Id}");
            playerDatas.Remove(changeEvent.Value.Id);
            Logger.Instance.Log(this, $"Player {changeEvent.Value.Id} data removed");
        }
        else if (changeEvent.Type == NetworkListEvent<NetworkTuple_PlayerData>.EventType.Add)
        {
            playerDatas[changeEvent.Value.Id] = changeEvent.Value.PlayerData;
            Logger.Instance.Log(this, $"Player {changeEvent.Value.Id} data added: {changeEvent.Value.PlayerData}");
        }
        else if (changeEvent.Type == NetworkListEvent<NetworkTuple_PlayerData>.EventType.Value)
        {
            playerDatas[changeEvent.Value.Id] = changeEvent.Value.PlayerData;
            Logger.Instance.Log(this, $"Player {changeEvent.Value.Id} data updated: {changeEvent.Value.PlayerData}");
        }
        else
        {
            Logger.Instance.LogError(this, $"Unexpected change event type: {changeEvent.Type}");
        }
    }

    public override void OnNetworkSpawn()
    {
        Logger.Instance.Log(this, "Spawned");

        Init();

        (byte, byte) avatarAndFrameIndexes = GetAvatarAndFrameIndexes();
        AddPlayerData(new PlayerData(avatarAndFrameIndexes.Item1, avatarAndFrameIndexes.Item2));

        if (IsServer)
            RelayManager.Instance.OnClientDisconnect.AddListener(OnClientDisconnect);
            //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        // server

        //for (int i = 0; i < playerDatasList.Count; i++)
        //{
        //    if (playerDatasList[i].Id == clientId)
        //        playerDatasList.RemoveAt(i);
        //}
    }

    /// <summary> </summary>
    /// <returns> First is Avatar index, second is Frame index </returns>
    private (byte, byte) GetAvatarAndFrameIndexes()
    {
        (byte, byte) result = (7, 0);

        if (CustomController.Instance == null)
            return result;

        for (int i = 0; i < avatarsAndFramesStorage.products.Count; i++)
        {
            if (avatarsAndFramesStorage.products[i].Data.Type == ItemType.Avatars)
            {
                if (CustomController.Instance.Custom[(int)ItemType.Avatars].Data.productName == avatarsAndFramesStorage.products[i].Data.productName)
                {
                    result.Item1 = (byte)i;
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
                    result.Item2 = (byte)i;
                    break;
                }
            }
        }

        return result;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerDataServerRpc(PlayerData playerData, ServerRpcParams rpcParams)
    {
        playerDatasList.Add(new NetworkTuple_PlayerData((byte)rpcParams.Receive.SenderClientId, playerData));
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerDataServerRpc(PlayerData playerData, ServerRpcParams rpcParams)
    {
        int index = 0;
        for (; index < playerDatasList.Count; index++)
            if (playerDatasList[index].Id == rpcParams.Receive.SenderClientId)
                break;

        if (index > playerDatasList.Count)
        {
            Logger.Instance.LogWarning(this, $"{nameof(playerDatasList)} doesn't contain player {rpcParams.Receive.SenderClientId} data");
            return;
        }

        playerDatas[rpcParams.Receive.SenderClientId] = playerData;
        playerDatasList[index] = new NetworkTuple_PlayerData((byte)rpcParams.Receive.SenderClientId, playerData);
    }

    public void AddPlayerData(PlayerData playerData)
    {
        ulong localClientId = NetworkManager.LocalClientId;

        if (playerDatas.ContainsKey(localClientId))
        {
            Logger.Instance.LogWarning(this, $"Player data already stored. Use {nameof(ChangePlayerData)} instead");
            return;
        }

        playerDatas[localClientId] = playerData;

        if (IsServer)
            playerDatasList.Add(new NetworkTuple_PlayerData((byte)localClientId, playerData));
        else
            AddPlayerDataServerRpc(playerData, new ServerRpcParams());
    }

    public void ChangePlayerData(PlayerData playerData)
    {
        ulong localClientId = NetworkManager.LocalClientId;

        if (!playerDatas.ContainsKey(localClientId))
        {
            Logger.Instance.LogWarning(this, $"{nameof(playerDatas)} doesn't contain player data. First you need to call {nameof(AddPlayerData)}");
            return;
        }

        if (IsServer)
            playerDatasList.Add(new NetworkTuple_PlayerData((byte)localClientId, playerData));
        else
            ChangePlayerDataServerRpc(playerData, new ServerRpcParams());
    }

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
}