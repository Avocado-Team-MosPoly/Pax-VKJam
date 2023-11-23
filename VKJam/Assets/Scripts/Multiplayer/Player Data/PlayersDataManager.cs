using System;
using System.Collections.Generic;
using Unity.Netcode;

public class PlayersDataManager : NetworkBehaviour
{
    public static PlayersDataManager Instance { get; private set; }

    public IReadOnlyDictionary<ulong, PlayerData> PlayerDatas => playerDatas;

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

        if (!playerDatas.ContainsKey(NetworkManager.LocalClientId))
            AddPlayerData(new PlayerData(false));
        else
            Logger.Instance.Log($"[{nameof(PlayersDataManager)}] Your data already stored");
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

        Logger.Instance.Log($"[{nameof(PlayersDataManager)}] {localClientId}. {playerDatas.Count}");

        if (playerDatas.ContainsKey(localClientId))
        {
            Logger.Instance.LogWarning($"[{nameof(PlayersDataManager)}] Player data already stored. You need {nameof(ChangePlayerData)} method");
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