using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TokensManager : MonoBehaviour
{
    public static int TokensCount { get; private set; }
    public static int TokensCountWinnedCurrentRound { get; private set; }
    public static int TokensCountLoosedCurrentRound { get; private set; }

    public static UnityEvent OnAddTokens;
    public static UnityEvent OnRemoveTokens;

    [SerializeField] private GameObject tokenPrefab;
    [SerializeField] private Transform tokenSpawnTransform;
    
    private static List<Token> tokensOnScene;
    private static TokensManager instance;
    
    private TextMeshProUGUI tokensCount;
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
        tokensCount = GetComponent<TextMeshProUGUI>();
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
                //token.Spawn();
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

        tokensTotal.text = Mathf.Abs(TokensCount).ToString();

        TokensCountWinnedCurrentRound = 0;
        TokensCountLoosedCurrentRound = 0;
    }

    public static void AccrueTokens()
    {
        instance.Summary();

        instance.SpawnTokens(TokensCount - tokensOnScene.Count);
        instance.tokensCount.text = "X" + TokensCount.ToString();
    }
    
    public static void AddTokens(int value)
    {
        TokensCountWinnedCurrentRound += value;
    }

    public static void RemoveTokens(int value)
    {
        TokensCountLoosedCurrentRound += value;
    }
}