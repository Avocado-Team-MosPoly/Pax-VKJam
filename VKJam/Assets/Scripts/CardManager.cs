using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CardManager : MonoBehaviour
{
    [SerializeField] private Card cardPrefab;

    //[SerializeField, Tooltip("Monster sprite on scene")] private SpriteRenderer monsterSpriteRenderer;

    [SerializeField] private CardDifficulty[] spawnedCardDifficulties =
    {
        CardDifficulty.Dangerous,
        CardDifficulty.Dangerous,
        CardDifficulty.Murderous
    };

    [Header("Scriptable Objects")]
    private List<CardSO> cardSOArray=new();
    [SerializeField] private PackCardSO packCardSO;
    private Dictionary<CardDifficulty, List<CardSO>> cardSODictionary = new();
    private List<CardSO> usedCardSO = new();
    private int cardSOCount;

    private CardSO choosedCardSO;
    
    [Header("Where cards should spawn")]
    [SerializeField] private Transform[] spawnTransforms;
    private List<Transform> occupiedSpawnTransforms = new();
    private List<Card> cardInstances = new();

    public UnityEvent<byte> OnChooseCard = new();

    private void OnValidate()
    {
        if (spawnedCardDifficulties.Length != spawnTransforms.Length)
            LogWarning("Please set the same number of elements in [ Spawned Card Difficulties ] and [ Spawn Transforms ] arrays.");
    }

    private void Awake()
    {
        StartCoroutine(start());
        //Array.Clear(cardSOArray, 0, cardSOCount);

        Card.OnChoose.AddListener(ChooseCardInstance);
        Card.OnSelect.AddListener(DisableInteractable);
        GameManager.Instance.RoleManager.OnGuesserSetted.AddListener(TakePack);
        GameManager.Instance.RoleManager.OnPainterSetted.AddListener(TakePack);
    }

    private IEnumerator start()
    {
        yield return new WaitForSeconds(0.1f);
        TakePack();
        foreach (CardDifficulty cardDifficulty in Enum.GetValues(typeof(CardDifficulty)))
            cardSODictionary.Add(cardDifficulty, new List<CardSO>());
        foreach (CardSO cardSO in cardSOArray)
            cardSODictionary[cardSO.Difficulty].Add(cardSO);

        cardSOCount = cardSOArray.Count;
    }

    public void TakePack()
    {
        cardSOArray.Clear();
        for (int i = 0; i < PackManager.Instance.PlayersOwnedCard[GameManager.Instance.PainterId].Count; i++)
        {
            if (PackManager.Instance.PlayersOwnedCard[GameManager.Instance.PainterId][i] == true)
            {
                cardSOArray.Add(packCardSO.CardInPack[i].Card);
            }
        }       
    }
    public void SetPack(PackCardSO _packCardSO)
    {
        packCardSO = _packCardSO;
    }

    private void OnEnable()
    {
        foreach (CardDifficulty cardDifficulty in spawnedCardDifficulties)
            SpawnCard(cardDifficulty);
    }

    #region Spawn

    private void SpawnCard(CardDifficulty cardDifficulty)
    {
        SpawnCard(GetUnusedCardSO(cardDifficulty));
    }

    private void SpawnCard(CardSO cardSO)
    {
        if (cardSO == null)
        {
            LogWarning("Incorrect Card Scriptable Object");
            return;
        }

        Transform spawnTransform = GetFreeSpawnTransform();

        if (spawnTransform == null)
        {
            LogError("Don't have free spawn position");
            return;
        }

        Card cardInstance = Instantiate(cardPrefab, spawnTransform.position, spawnTransform.rotation, spawnTransform);
        cardInstance.SetCardSO(cardSO);

        Log($"{cardInstance.CardSO.Id} spawned as {cardInstance.CardSO.Difficulty} card");

        occupiedSpawnTransforms.Add(spawnTransform);
        usedCardSO.Add(cardSO);
        cardInstances.Add(cardInstance);
    }

    #endregion
    #region Get

    private Transform GetFreeSpawnTransform()
    {
        if (occupiedSpawnTransforms.Count == spawnTransforms.Length)
            return null;

        int transformIndex = UnityEngine.Random.Range(0, spawnTransforms.Length);
        while (occupiedSpawnTransforms.Contains(spawnTransforms[transformIndex]))
        {
            transformIndex++;
            if (transformIndex >= spawnTransforms.Length)
                transformIndex = 0;
        }
        
        return spawnTransforms[transformIndex];
    }

    private CardSO GetUnusedCardSO(CardDifficulty cardDifficulty)
    {
        List<CardSO> cardSOs = cardSODictionary[cardDifficulty];
        int otherCardSOCount = cardSOCount - cardSOs.Count;

        if (usedCardSO.Count - otherCardSOCount >= cardSOs.Count)
            return null;

        int startCardIndex = UnityEngine.Random.Range(0, cardSOs.Count);
        int cardIndex = startCardIndex;
        Log(cardIndex.ToString());

        while (usedCardSO.Contains(cardSOs[cardIndex]))
        {
            cardIndex++;
            if (cardIndex >= cardSOs.Count)
                cardIndex = 0;

            if (cardIndex == startCardIndex)
                return null;
        }

        return cardSOs[cardIndex];
    }

    public byte GetCardSOIndex(CardSO cardSO) => (byte)cardSOArray.IndexOf(cardSO);
    public CardSO GetCardSOByIndex(ushort cardSOIndex) => cardSOArray[cardSOIndex];

    #endregion
    #region Interaction

    private void DestroyCardInstances()
    {
        occupiedSpawnTransforms = new();

        foreach (Card instance in cardInstances)
        {
            Destroy(instance.gameObject);
        }

        cardInstances.Clear();
    }

    /// <summary> Disable [ Interactable ] on all spawned cards excluding parameter </summary>
    private void DisableInteractable(Card excludedCard)
    {
        foreach (Card cardInstance in cardInstances)
            if (cardInstance != excludedCard)
                cardInstance.GetComponent<Interactable>().SetInteractable(false);
    }

    public void ChooseCardInstance(Card card)
    {
        if (cardInstances.Contains(card))
        {
            choosedCardSO = card.CardSO;
            usedCardSO.Add(card.CardSO);
            //monsterSpriteRenderer.sprite = card.CardSO.MonsterTexture;

            OnChooseCard.Invoke(GetCardSOIndex(card.CardSO));
            Log(card.CardSO.Id);

            DestroyCardInstances();
            //CameraBackButton.SetActive(false);
            //CameraButtonAfterChoosingCard.SetActive(true);
        }
    }

    public void ResetMonsterSprite()
    {
        //monsterSpriteRenderer.sprite = null;
    }

    #endregion
    #region Logs

    private void Log(string message) => Debug.Log($"[CardManager] {message}");
    private void LogWarning(string message) => Debug.LogWarning($"[CardManager] {message}");
    private void LogError(string message) => Debug.LogError($"[CardManager] {message}");
    
    #endregion
}