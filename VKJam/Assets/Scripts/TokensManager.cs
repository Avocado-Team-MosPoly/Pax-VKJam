using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TokensManager : MonoBehaviour
{
    public static int TokensCount { get; private set; }

    public static UnityEvent OnAddTokens;
    public static UnityEvent OnRemoveTokens;

    [SerializeField] private GameObject tokenPrefab;
    [SerializeField] private Transform tokenSpawnTransform;
    
    private static List<Token> tokensOnScene;
    private static TokensManager instance;
    
    private TextMeshProUGUI tokensCountTMPro;

    private void Start()
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
        tokensCountTMPro = GetComponent<TextMeshProUGUI>();
    }

    private void SpawnTokens(int count)
    {
        if (instance.tokenPrefab && instance.tokenSpawnTransform)
        {
            Token token;
            for (int i = 0; i < count; i++)
            {
                token = Instantiate(instance.tokenPrefab, instance.tokenSpawnTransform).GetComponent<Token>();
                //token.Spawn();
                tokensOnScene.Add(token);
            }
        }
    }

    private void DeleteExcessTokens()
    {
        if (tokensOnScene.Count > 0)
        {
            for (int i = tokensOnScene.Count - 1; i >= TokensCount; i--)
            {
                tokensOnScene[i].Destruct();
                tokensOnScene.RemoveAt(i);
            }
        }
    }

    public static void AddTokens(int value)
    {
        TokensCount += value;

        instance.tokensCountTMPro.text = "X" + TokensCount.ToString();
        instance.SpawnTokens(value);
    }

    public static void RemoveTokens(int value)
    {
        TokensCount -= value;

        if (TokensCount < 0)
        {
            TokensCount = 0;
        }

        instance.tokensCountTMPro.text = "X" + TokensCount.ToString();
        instance.DeleteExcessTokens();
    }
}