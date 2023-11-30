using UnityEngine;
using System.Linq;
[System.Serializable]
public struct CardSystem
{
    public int CardDBIndex;
    public bool CardIsInOwn;
    public CardSO Card;
}
[CreateAssetMenu()]
public class PackCardSO : ScriptableObject
{
    
    [SerializeField] private string PackName;
    [SerializeField] private bool PackIsInOwn;
    public int PackDBIndex;
    
    
    public CardSystem[] CardInPack;

    [ContextMenu("Sort alphabetically")]
    private void Sorting()
     {
         string temp = "";
         for (int j = 0; j < CardInPack.Length; ++j)
         {
             CardInPack[j].CardDBIndex = j;
             temp += CardInPack[j].Card.id + "\n";
         }
         CardInPack = CardInPack.OrderBy(cardSystem => cardSystem.Card.id).ToArray();
         Debug.Log(temp);
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
    [ContextMenu("Set all CardIsInOwn - false")]
    public void ResetOwning()
    {
        return;
        for (int j = 0; j < CardInPack.Length; ++j)
        {
            CardInPack[j].CardIsInOwn = false;
        }
    }
}
