using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TokensManager : MonoBehaviour
{
    public static int TokensCount { get; private set; }
    
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

    public static void AddScore(int value)
    {
        TokensCount += value;

        instance.tokensCountTMPro.text = "X" + TokensCount.ToString();

        if (instance.tokenPrefab && instance.tokenSpawnTransform)
        {
            Token token;
            for (int i = 0; i < value; i++)
            {
                token = Instantiate(instance.tokenPrefab, instance.tokenSpawnTransform).GetComponent<Token>();
                token.OnSpawn();
                tokensOnScene.Add(token);
            }
        }
    }

    public static void RemoveScore(int value)
    {
        TokensCount -= value;

        if (TokensCount < 0)
        {
            TokensCount = 0;
        }

        instance.tokensCountTMPro.text = "X" + TokensCount.ToString();

        if (tokensOnScene.Count > 0)
        {
            for (int i = tokensOnScene.Count - 1; i >= TokensCount; i--)
            {
                tokensOnScene[i].OnDestruct();
                tokensOnScene.RemoveAt(i);
            }
        }
    }
}