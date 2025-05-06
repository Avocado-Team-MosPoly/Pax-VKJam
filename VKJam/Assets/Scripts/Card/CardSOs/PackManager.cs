using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PackManager : MonoBehaviour
{
    public CardPackSO Active;
    public List<CardPackSO> All = new();
    public Dictionary<ulong, List<bool>> PlayersOwnedCard = new();

    public static PackManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public IEnumerator Init(string cardIndexes)
    {
        if (Active == null)
        {
            Logger.Instance.LogError(this, $"{nameof(Active)} is null. Must be default pack");
            yield break;
        }
        AddPack(Active);

        if (string.IsNullOrEmpty(cardIndexes))
        {
            Logger.Instance.LogError(this, $"{nameof(cardIndexes)} is null or empty");
            yield break;
        }

        string[] splittedIndexes = cardIndexes.Split('\n');

        foreach (var card in Active.CardInPack)
        {
            foreach (var index in splittedIndexes)
            {
                if (card.CardDBIndex.ToString() == index)
                {
                    card.CardIsInOwn = true;
                    break;
                }
                else
                {
                    card.CardIsInOwn = false;
                }
            }
        }

        
        
        Logger.Instance.Log(this, "Initialized");
        yield break;
    }

    public void SetPack(string cardPackName)
    {
        if (TryGet(cardPackName, out var pack))
        {
            Active = pack;
            Logger.Instance.Log(this, $"Pack '{cardPackName}' setted");
        }
        else
        {
            Logger.Instance.LogError(this, $"Failed to set pack '{cardPackName}'. Not found");
        }
    }

    public void SetPack(CardPackSO cardPack)
    {
        Active = cardPack;
        Logger.Instance.Log(this, $"Pack '{cardPack.PackName}' setted");

        AddPack(cardPack);
    }

    public void AddPack(CardPackSO cardPack)
    {
        if (Contains(cardPack))
            return;

        All.Add(cardPack);
    }

    public bool Contains(CardPackSO cardPack)
    {
        foreach(CardPackSO pack in All)
            if (cardPack == pack)
                return true;

        return false;
    }

    public bool TryGet(string cardPackName, out CardPackSO cardPack)
    {
        cardPack = null;

        foreach (CardPackSO pack in All)
        {
            if (cardPackName == pack.PackName)
            {
                cardPack = pack;
                return true;
            }
        }

        return false;
    }
}