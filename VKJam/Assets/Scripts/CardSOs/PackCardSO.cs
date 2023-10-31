using System;
using UnityEngine;
[CreateAssetMenu()]
public class PackCardSO : ScriptableObject
{
    [System.Serializable]
    public struct CardSystem
    {
        public int CardDBIndex;
        public bool CardIsInOwn;
        public CardSO Card;

        private static int currentIndex = 0;
    }
    [SerializeField] private string PackName;
    [SerializeField] private bool PackIsInOwn;
    [SerializeField] private int PackDBIndex;

    public CardSystem[] CardInPack;
     
    /*private void OnValidate()
    {
        string temp = "";
        for(int j =0;j< CardInPack.Length; ++j)
        {
            CardInPack[j].CardDBIndex = j;
            temp += CardInPack[j].Card.id + "\n";
        }
        Debug.Log(temp);
    }*/
}
