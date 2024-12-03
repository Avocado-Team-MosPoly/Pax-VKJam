using UnityEngine;
using System.Linq;

[System.Serializable]
public class CardSystem
{
    public int CardDBIndex;
    public bool CardIsInOwn;
    public CardSO Card;
}

[CreateAssetMenu()]
public class CardPackSO : ScriptableObject
{
    public string PackName;
    public bool PackIsInOwn;
    public int PackDBIndex;

    public CardSystem[] CardInPack;

    public BaseCardInfo SearchCardById(int idCard)
    {
        foreach (var card in CardInPack)
            if (card.CardDBIndex == idCard)
                return card.Card;

        return null;
    }

    public CardSystem SearchCardSystemById(int idCard)
    {
        foreach (var card in CardInPack)
            if (card.CardDBIndex == idCard)
                return card;

        return null;
    }

    [ContextMenu("Sort alphabetically")]
    private void Sorting()
    {
        string temp = "";

        for (int j = 0; j < CardInPack.Length; ++j)
        {
            CardInPack[j].CardDBIndex = j;
            temp += CardInPack[j].Card.Id + "\n";
        }

        CardInPack = CardInPack.OrderBy(cardSystem => cardSystem.Card.Id).ToArray();
        Debug.Log(temp);
    }

    [ContextMenu("Set all CardIsInOwn - false")]
    private void ResetOwning()
    {
        for (int j = 0; j < CardInPack.Length; ++j)
        {
            CardInPack[j].CardIsInOwn = false;
        }
    }

    /*public PackCardSO PackDataOwnering()
    {
        string res = Php_Connect.Request_WhichCardInPackOwnering(PackDBIndex);
        string[] result = res.Split();
        Debug.Log(res);
        foreach (var current in result)
        {
            if (current == "") break;
            CardInPack[int.Parse(current)].CardIsInOwn = true;
        }
        return this;
    }*/
}