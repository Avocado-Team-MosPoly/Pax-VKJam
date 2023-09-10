using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using Unity.Netcode;

// needs rework
public class TokenManager : NetworkBehaviour
{
    public static int TokensCount { get; private set; }
    public static int TokensCountWinnedCurrentRound { get; private set; }
    public static int TokensCountLoosedCurrentRound { get; private set; }

    public static UnityEvent OnAddTokens;
    public static UnityEvent OnRemoveTokens;

    [SerializeField] private GameObject tokenPrefab;
    [SerializeField] private Transform tokenSpawnTransform;
    
    private static List<Token> tokensOnScene;
    private static TokenManager instance;
    
    [SerializeField] private TextMeshProUGUI tokensCount;
    [SerializeField] private TextMeshProUGUI tokensWinned;
    [SerializeField] private TextMeshProUGUI tokensLoosed;
    [SerializeField] private TextMeshProUGUI tokensTotal;

    private void Awake()
    {
        if (instance)
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            instance = this;
        }

        tokensOnScene = new List<Token>();
    }

    private void SpawnTokens(int count)
    {
        if (count < 0)
        {
            DeleteExcessTokens();
        }

        if (tokenPrefab && tokenSpawnTransform)
        {
            Token token;
            for (int i = 0; i < count; i++)
            {
                token = Instantiate(tokenPrefab, tokenSpawnTransform).GetComponent<Token>();
                tokensOnScene.Add(token);
            }
        }
    }

    private void DeleteExcessTokens()
    {
        if (tokensOnScene.Count > 0)
        {
            if (TokensCount < 0)
            {
                foreach (Token token in tokensOnScene)
                    token.Destruct();

                tokensOnScene.Clear();
            }
            else
            {
                for (int i = tokensOnScene.Count - 1; i >= TokensCount; i--)
                {
                    tokensOnScene[i].Destruct();
                    tokensOnScene.RemoveAt(i);
                }
            }
        }
    }

    private void Summary()
    {
        tokensWinned.text = TokensCountWinnedCurrentRound.ToString();
        tokensLoosed.text = TokensCountLoosedCurrentRound.ToString();

        TokensCount += TokensCountWinnedCurrentRound - TokensCountLoosedCurrentRound;
        TokensCount = Mathf.Max(0, TokensCount);

        tokensTotal.text = "X" + TokensCount.ToString();

        TokensCountWinnedCurrentRound = 0;
        TokensCountLoosedCurrentRound = 0;
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
    }

    [ClientRpc]
    private void RemoveTokensClientRpc(byte value)
    {
        TokensCountLoosedCurrentRound += value;
    }

    public static void AccrueTokens()
    {
        instance.AccrueTokensClientRpc();
    }
    
    public static void AddTokens(int value)
    {
        instance.AddTokensClientRpc((byte)value);
    }

    public static void RemoveTokens(int value)
    {
        instance.RemoveTokensClientRpc((byte)value);
    }


    [ClientRpc]
    private void AddTokensToClientRpc(byte value, byte clientId)
    {
        TokensCountWinnedCurrentRound += value;
    }

    [ClientRpc]
    private void RemoveTokensToClientRpc(byte value, byte clientId)
    {
        TokensCountLoosedCurrentRound += value;
    }
    
    public static void AddTokensToClient(int value, byte clientId)
    {
        instance.AddTokensToClientRpc((byte)value, clientId);
    }

    public static void RemoveTokensToClient(int value, byte clientId)
    {
        instance.RemoveTokensToClientRpc((byte)value, clientId);
    }
}