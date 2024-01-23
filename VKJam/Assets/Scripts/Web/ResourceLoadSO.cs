using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ResourceLoadSO : ScriptableObject
{
    [System.Serializable]
    public struct CardURLs
    {
        public CardURLs(string _id)
        {
            id = _id;
            cardTextureURL = "";
            monsterTextureURL = "";
            monsterInBestiaryTextureURL = "";
        }

        public string id;
        public string cardTextureURL;
        public string monsterTextureURL;
        public string monsterInBestiaryTextureURL;
    }

    public PackCardSO[] cardPacks;

    public List<CardURLs> cardURLs;

    public string[] GetURLsByCardID(string id)
    {
        foreach(var e in cardURLs)
        {
            if (e.id == id)
                return new string[] { e.cardTextureURL, e.monsterTextureURL, e.monsterInBestiaryTextureURL };
        }
        return null;
    }

    [ContextMenu("Add cards to config")]
    private void AddCards()
    {
        foreach(var pack in cardPacks)
        {
            foreach(var card in pack.CardInPack)
            {
                if(GetURLsByCardID(card.Card.Id) == null)
                {
                    cardURLs.Add(new CardURLs(card.Card.Id));
                }
            }
        }
    }
}
