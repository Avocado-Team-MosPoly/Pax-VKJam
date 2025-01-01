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

    //[Header("Scriptable Objects")]
    private List<BaseCardSO> cardInfos = new();
    private List<BaseCardSO> usedCardInfos = new();
    private Dictionary<CardDifficulty, List<BaseCardSO>> cardInfoDictionary = new();

    private BaseCardSO choosedCardInfo;
    
    [Header("Where cards should spawn")]
    [SerializeField] private Transform[] spawnTransforms;
    private List<Transform> occupiedSpawnTransforms = new();
    private List<Card> cardInstances = new();

    public UnityEvent<byte> OnChooseCard = new();

    private void OnValidate()
    {
        if (spawnedCardDifficulties.Length != spawnTransforms.Length)
            LogWarning($"Set the same number of elements in [{nameof(spawnedCardDifficulties)}] and [{nameof(spawnTransforms)}] arrays.");
    }

    private void Awake()
    {
        StartCoroutine(InitCards());

        GameManager.Instance.RoleManager.OnGuesserSetted.AddListener(TakePack);
        GameManager.Instance.RoleManager.OnPainterSetted.AddListener(TakePack);
    }

    private IEnumerator InitCards()
    {
        yield return new WaitForSeconds(0.1f);
        TakePack();
    }

    public void TakePack()
    {
        cardInfos.Clear();
        cardInfoDictionary.Clear();

        foreach (CardDifficulty cardDifficulty in Enum.GetValues(typeof(CardDifficulty)))
            cardInfoDictionary.Add(cardDifficulty, new List<BaseCardSO>());

        for (int i = 0; i < PackManager.Instance.PlayersOwnedCard[GameManager.Instance.PainterId].Count; i++)
        {
            if (PackManager.Instance.PlayersOwnedCard[GameManager.Instance.PainterId][i] == true)
            {
                BaseCardSO cardInfo = PackManager.Instance.Active.CardInPack[i].Card;
                cardInfos.Add(cardInfo);
                cardInfoDictionary[cardInfo.Difficulty].Add(cardInfo);
            }
        }
    }

    private void OnEnable()
    {
        foreach (CardDifficulty cardDifficulty in spawnedCardDifficulties)
            SpawnCard(cardDifficulty);
    }

    #region Spawn

    private void SpawnCard(CardDifficulty cardDifficulty)
    {
        SpawnCard(GetUnusedCardInfo(cardDifficulty));
    }

    private void SpawnCard(BaseCardSO cardInfo)
    {
        if (cardInfo == null)
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
        cardInstance.SetCardInfo(cardInfo);

        cardInstance.OnChoose.AddListener(ChooseCardInstance);
        cardInstance.OnSelect.AddListener(DisableInteractable);

        Log($"{cardInstance.CardInfo.Id} spawned as {cardInstance.CardInfo.Difficulty} card");

        occupiedSpawnTransforms.Add(spawnTransform);
        usedCardInfos.Add(cardInfo);
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

    private BaseCardSO GetUnusedCardInfo(CardDifficulty cardDifficulty)
    {
        List<BaseCardSO> sameDifficultyCardInfos = cardInfoDictionary[cardDifficulty];
        int otherCardInfosCount = cardInfos.Count - sameDifficultyCardInfos.Count;

        if (usedCardInfos.Count - otherCardInfosCount >= sameDifficultyCardInfos.Count)
            return null;

        int startCardIndex = UnityEngine.Random.Range(0, sameDifficultyCardInfos.Count);
        int cardIndex = startCardIndex;
        Log(cardIndex.ToString());

        while (usedCardInfos.Contains(sameDifficultyCardInfos[cardIndex]))
        {
            cardIndex++;
            if (cardIndex >= sameDifficultyCardInfos.Count)
                cardIndex = 0;

            if (cardIndex == startCardIndex)
                return null;
        }

        return sameDifficultyCardInfos[cardIndex];
    }

    public byte GetCardInfoIndex(BaseCardSO cardInfo)
    {
        int index = cardInfos.IndexOf(cardInfo);

        if (index < 0)
        {
            Logger.Instance.LogError(this, $"{nameof(index)} below 0");
        }

        return (byte)index;
    }

    public BaseCardSO GetCardInfoByIndex(ushort cardInfoIndex)
    {
        if (cardInfoIndex < 0 || cardInfoIndex >= cardInfos.Count)
            Logger.Instance.LogError(this, new ArgumentOutOfRangeException($"{nameof(cardInfoIndex)} below 0"));

        return cardInfos[cardInfoIndex];
    }

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
    private void DisableInteractable(Card exclude)
    {
        foreach (Card cardInstance in cardInstances)
            if (cardInstance != exclude)
                cardInstance.GetComponent<Interactable>().SetInteractable(false);
    }

    public void ChooseCardInstance(Card card)
    {
        if (cardInstances.Contains(card))
        {
            choosedCardInfo = card.CardInfo;
            usedCardInfos.Add(card.CardInfo);
            //monsterSpriteRenderer.sprite = card.CardInfo.MonsterTexture;

            OnChooseCard.Invoke(GetCardInfoIndex(card.CardInfo));
            Log(card.CardInfo.Id);

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