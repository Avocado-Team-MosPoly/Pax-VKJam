using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayersDataManager : NetworkBehaviour
{
    public static PlayersDataManager Instance { get; private set; }

    public IReadOnlyDictionary<ulong, PlayerData> PlayerDatas => playerDatas;

    public storeSection AvatarsAndFramesStorage => avatarsAndFramesStorage;

    [SerializeField] private storeSection avatarsAndFramesStorage;

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
                Logger.Instance.Log($"[{nameof(PlayersDataManager)}] Player {tuple.Id} data added: {tuple.PlayerData})");
            }

            playerDatasList.OnListChanged += PlayerDatasList_OnListChanged;
        }
        else
        {
            Logger.Instance.LogError($"[{nameof(PlayersDataManager)}] Second {nameof(PlayersDataManager)} spawned");
            return;
        }
    }

    private void PlayerDatasList_OnListChanged(NetworkListEvent<NetworkTuple_PlayerData> changeEvent)
    {
        playerDatas[changeEvent.Value.Id] = changeEvent.Value.PlayerData;
        Logger.Instance.Log($"[{nameof(PlayersDataManager)}] Player {changeEvent.Value.Id} data added: {changeEvent.Value.PlayerData})");
    }

    public override void OnNetworkSpawn()
    {
        Logger.Instance.Log($"[{nameof(PlayersDataManager)}] Spawned");

        Init();

        (byte, byte) avatarAndFrameIndexes = GetAvatarAndFrameIndexes();
        AddPlayerData(new PlayerData(avatarAndFrameIndexes.Item1, avatarAndFrameIndexes.Item2));
    }

    /// <summary> </summary>
    /// <returns> First is Avatar index, second is Frame </returns>
    private (byte, byte) GetAvatarAndFrameIndexes()
    {
        (byte, byte) result = (1, 0);

        //if (CustomController._executor == null)
            return result;

        //Logger.Instance.LogError("");
        //Logger.Instance.Log(CustomController._executor.Custom[(int)ItemType.Avatars].Data.productName);
        //Logger.Instance.LogError("");
        for (int i = 0; i < avatarsAndFramesStorage.products.Count; i++)
        {
            if (avatarsAndFramesStorage.products[i].Data.Type == ItemType.Avatars)
            {
                //Logger.Instance.Log(avatarsAndFramesStorage.products[i].Data.productName);
                if (CustomController._executor.Custom[(int)ItemType.Avatars].Model == avatarsAndFramesStorage.products[i].Model)
                {
                    result.Item1 = (byte)i;
                    break;
                }
            }
        }
        //Logger.Instance.LogError("");
        //Logger.Instance.Log(CustomController._executor.Custom[(int)ItemType.AvatarFrame].Data.productName);
        //Logger.Instance.LogError("");
        for (int i = 0; i < avatarsAndFramesStorage.products.Count; i++)
        {
            if (avatarsAndFramesStorage.products[i].Data.Type == ItemType.AvatarFrame)
            {
                //Logger.Instance.Log(avatarsAndFramesStorage.products[i].Data.productName);
                if (CustomController._executor.Custom[(int)ItemType.AvatarFrame].Model == avatarsAndFramesStorage.products[i].Model)
                {
                    result.Item1 = (byte)i;
                    break;
                }
            }
        }
        //Logger.Instance.LogWarning(result);

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
            Logger.Instance.LogWarning($"[{nameof(PlayersDataManager)}] {nameof(playerDatasList)} doesn't contain player {rpcParams.Receive.SenderClientId} data");
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
            Logger.Instance.LogWarning($"[{nameof(PlayersDataManager)}] Player data already stored. Use {nameof(ChangePlayerData)} instead");
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
            Logger.Instance.LogWarning($"[{nameof(PlayersDataManager)}] {nameof(playerDatas)} doesn't contain player data. First you need to call {nameof(AddPlayerData)}");
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