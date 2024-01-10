using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using Unity.Netcode;
using System.Collections;
using System;
using Unity.VisualScripting;

// needs rework
public class TokenManager : NetworkBehaviour
{
    public static int TokensCount { get; private set; }
    public static int TokensCountWinnedCurrentRound { get; private set; }
    public static int TokensCountLosedCurrentRound { get; private set; }

    public static UnityEvent OnAddTokens;
    public static UnityEvent OnRemoveTokens;

    [SerializeField] private GameObject table;

    [Header("Spawn Tokens")]
    [SerializeField] private GameObject tokenPrefab;
    [SerializeField] private Transform tokenSpawnTransform;
    [SerializeField] private Vector2 tokenSpawnRect;

    [Header("Tokens Text Labels")]
    [SerializeField] private TextMeshProUGUI tokensCount;
    [SerializeField] private TextMeshProUGUI tokensWinned;
    [SerializeField] private TextMeshProUGUI tokensLoosed;
    [SerializeField] private TextMeshProUGUI tokensTotal;

    private Dictionary<ulong, int> playersTokens = new();
    private static List<GameObject> tokensOnScene;

    private static TokenManager instance;

    private void Awake()
    {
        if(CustomController.Executor != null)
        {
            tokenPrefab = CustomController.Executor.Custom[(int)ItemType.Token].Model;
        }
        if (instance)
        {
            if (instance != this)
                Destroy(gameObject);
        }
        else
            instance = this;

        tokensOnScene = new List<GameObject>();

    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.R))
    //    {
    //        DeleteExcessTokens();
    //        SpawnTokens(10);
    //    }
    //}

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            playersTokens.Add(clientId, 0);
    }

    private (IReadOnlyList<ulong>, IReadOnlyList<int>) SortPlayersTokensDictionary()
    {
        List<ulong> clientIds = new();
        List<int> tokens = new();

        foreach (ulong clientId in playersTokens.Keys)
            clientIds.Add(clientId);
        foreach (int tokenCount in playersTokens.Values)
            tokens.Add(tokenCount);

        for (int i = 0; i < tokens.Count - 1; i++)
        {
            for (int j = i + 1; j < tokens.Count; j++)
            {
                if (tokens[i] > tokens[j])
                    continue;

                int tokensCount = tokens[i];
                ulong clientId = clientIds[i];

                tokens[i] = tokens[j];
                clientIds[i] = clientIds[j];

                tokens[j] = tokensCount;
                clientIds[j] = clientId;
            }
        }

        return (clientIds, tokens);
    }

    private void SpawnTokens(int count)
    {
        if (count < 0)
        {
            DeleteExcessTokens();
            return;
        }

        if (tokenPrefab && tokenSpawnTransform)
        {
            Vector2 halfTokenSpawn = tokenSpawnRect / 2;
            GameObject tokenGameObject;

            for (int i = 0; i < count; i++)
            {
                Vector3 localPosition = new(
                    UnityEngine.Random.Range(-halfTokenSpawn.x, halfTokenSpawn.x),
                    0f,
                    UnityEngine.Random.Range(-halfTokenSpawn.y, halfTokenSpawn.y)
                    );

                tokenGameObject = Instantiate(tokenPrefab, tokenSpawnTransform);
                tokenGameObject.transform.localPosition = localPosition;

                tokensOnScene.Add(tokenGameObject);
            }
        }

        GameManager.Instance.SoundList.Play("Falling token");
    }

    private void DeleteExcessTokens()
    {
        if (tokensOnScene.Count > 0)
        {
            if (TokensCount < 0)
            {
                foreach (var token in tokensOnScene)
                    Destroy(token);

                tokensOnScene.Clear();
            }
            else
            {
                for (int i = tokensOnScene.Count - 1; i >= TokensCount; i--)
                {
                    Destroy(tokensOnScene[i]);
                    tokensOnScene.RemoveAt(i);
                }
            }
        }
    }

    private void Summary()
    {
        tokensWinned.text = TokensCountWinnedCurrentRound.ToString();
        tokensLoosed.text = TokensCountLosedCurrentRound.ToString();

        TokensCount += (TokensCountWinnedCurrentRound - TokensCountLosedCurrentRound);
        TokensCount = Mathf.Max(0, TokensCount);

        tokensTotal.text = "X" + TokensCount.ToString();

        TokensCountWinnedCurrentRound = 0;
        TokensCountLosedCurrentRound = 0;
    }

    [ClientRpc]
    private void AccrueTokensClientRpc()
    {
        instance.Summary();

        instance.SpawnTokens(TokensCount - tokensOnScene.Count);
        instance.tokensCount.text = "X" + TokensCount.ToString();
    }

    [ClientRpc]
    private void AddTokensClientRpc(byte value)
    {
        TokensCountWinnedCurrentRound += value;

        LogAdd(value);
    }

    [ClientRpc]
    private void RemoveTokensClientRpc(byte value)
    {
        TokensCountLosedCurrentRound += value;

        LogRemove(value);
    }

    public static void AccrueTokens()
    {
        instance.AccrueTokensClientRpc();
    }

    public static void AddTokensToAll(int value)
    {
        int tokensToAdd = value / NetworkManager.Singleton.ConnectedClientsIds.Count;

        instance.AddTokensClientRpc((byte)tokensToAdd);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            instance.playersTokens[clientId] += tokensToAdd;
    }

    public static void RemoveTokensFromAll(int value)
    {
        int tokensToRemove = value / NetworkManager.Singleton.ConnectedClientsIds.Count;

        instance.RemoveTokensClientRpc((byte)tokensToRemove);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            instance.playersTokens[clientId] -= tokensToRemove;

            if (instance.playersTokens[clientId] < 0)
                instance.playersTokens[clientId] = 0;
        }
    }

    [ClientRpc]
    private void AddTokensToClientRpc(byte value, byte clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;

        TokensCountWinnedCurrentRound += value;

        LogAdd(value);
    }

    [ClientRpc]
    private void RemoveTokensToClientRpc(byte value, byte clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;

        TokensCountLosedCurrentRound += value;
        
        LogRemove(value);
    }
    
    public static void AddTokensToClient(int value, byte clientId)
    {
        instance.AddTokensToClientRpc((byte)value, clientId);

        instance.playersTokens[clientId] += value;
    }

    public static void RemoveTokensToClient(int value, byte clientId)
    {
        instance.RemoveTokensToClientRpc((byte)value, clientId);

        instance.playersTokens[clientId] -= value;

        if (instance.playersTokens[clientId] < 0)
            instance.playersTokens[clientId] = 0;
    }

    public static void OnCompetitiveGameEnd()
    {
        IReadOnlyList<ulong> clientIds;
        IReadOnlyList<int> tokens;
        (clientIds, tokens) = instance.SortPlayersTokensDictionary();

        switch (clientIds.Count)
        {
            case 2:
                RemoveTokensToClient(tokens[1], (byte)clientIds[1]);
                break;
            case 3:
                RemoveTokensToClient((int)(tokens[1] * 0.7f), (byte)clientIds[1]);
                RemoveTokensToClient(tokens[2], (byte)clientIds[2]);
                break;
            case 4:
                RemoveTokensToClient((int)(tokens[1] * 0.5f), (byte)clientIds[1]);
                RemoveTokensToClient((int)(tokens[2] * 0.75f), (byte)clientIds[2]);
                RemoveTokensToClient(tokens[3], (byte)clientIds[3]);
                break;
        }

        AddTokensToClient(tokens[0], (byte)clientIds[0]);
    }

    public static IEnumerator OnGameEnded()
    {
        Action OnCompleted = () =>
        {
            Debug.Log("Coins saved to DB");
        };

        yield return instance.StartCoroutine(Php_Connect.Request_TokenWin(TokensCount, OnCompleted, null));
    }

    /// <summary> Dont use in Update </summary>
    public static (ulong, int) GetClientIdWithMaxTokens()
    {
        IReadOnlyList<ulong> clientIds;
        IReadOnlyList<int> tokens;
        (clientIds, tokens) = instance.SortPlayersTokensDictionary();

        return (clientIds[0], tokens[0]);
    }

    public static void ResetData()
    {
        TokensCount = 0;
        TokensCountWinnedCurrentRound = 0;
        TokensCountLosedCurrentRound = 0;

        if (tokensOnScene != null)
            tokensOnScene.Clear();
    }

    #region Log

    private void LogAdd(int value)
    {
        Logger.Instance.Log(this, $"Add {value} token(-s). Total added tokens: {TokensCountWinnedCurrentRound}");
    }

    private void LogRemove(int value)
    {
        Logger.Instance.Log(this, $"Remove {value} token(-s). Total removed tokens: {TokensCountLosedCurrentRound}");
    }

    #endregion

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Vector3 tokenSpawnBox = new Vector3(tokenSpawnRect.x, 0f, tokenSpawnRect.y);
    //    Gizmos.DrawWireCube(tokenSpawnTransform.position, tokenSpawnBox);

    ////    Vector3 prev = tokenSpawnTransform.position;
    ////    Vector3 current = tokenSpawnTransform.position + new Vector3(tokenSpawnRect.x, 0f, 0f);
    ////    current = tokenSpawnTransform.right * current.magnitude;
    ////    Gizmos.DrawLine(prev, current);

    ////    prev = current;
    ////    current.z += tokenSpawnRect.y;
    ////    current = tokenSpawnTransform.forward * current.magnitude;
    ////    Gizmos.DrawLine(prev, current);

    ////    prev = current;
    ////    current.x = tokenSpawnTransform.position.x;
    ////    current = -tokenSpawnTransform.right * current.magnitude;
    ////    Gizmos.DrawLine(prev, current);

    ////    prev = current;
    ////    current = tokenSpawnTransform.position;
    ////    Gizmos.DrawLine(prev, current);
    //}
}