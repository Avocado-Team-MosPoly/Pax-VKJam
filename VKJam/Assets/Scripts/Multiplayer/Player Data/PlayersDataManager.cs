using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayersDataManager : NetworkBehaviour
{
    public static PlayersDataManager Instance { get; private set; }

    public IReadOnlyDictionary<ulong, PlayerData> PlayerDatas => playerDatas;

    private Dictionary<ulong, PlayerData> playerDatas = new();

    private void Init()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Second Player Data Manager spawned");
            return;
        }
    }

    public override void OnNetworkSpawn()
    {
        Init();

        if (!playerDatas.ContainsKey(NetworkManager.LocalClientId))
            AddPlayerData(new PlayerData(false));
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerDataServerRpc(PlayerData playerData, ServerRpcParams rpcParams)
    {
        playerDatas[rpcParams.Receive.SenderClientId] = playerData;

        AddPlayerDataClientRpc(playerData, (byte)rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void AddPlayerDataClientRpc(PlayerData playerData, byte playerClientId)
    {
        if (playerClientId == NetworkManager.LocalClientId || IsServer)
            return;

        playerDatas[playerClientId] = playerData;

        Debug.Log("added player " + playerClientId + " data: " + playerData);
        Debug.Log(playerDatas.Count);
    }

    public void AddPlayerData(PlayerData playerData)
    {
        ServerRpcParams rpcParams = new ServerRpcParams();

        if (playerDatas.ContainsKey(rpcParams.Receive.SenderClientId))
        {
            Debug.LogWarning("Player Datas contains key " + rpcParams.Receive.SenderClientId);
            if (playerDatas[rpcParams.Receive.SenderClientId].Equals(playerData))
                return;

           
        }
        playerDatas[rpcParams.Receive.SenderClientId] = playerData;
        Debug.Log("added player data: " + playerData);
        Debug.Log(playerDatas.Count);

        if (IsServer)
            AddPlayerDataClientRpc(playerData, (byte)rpcParams.Receive.SenderClientId);
        else
            AddPlayerDataServerRpc(playerData, rpcParams);
    }
}