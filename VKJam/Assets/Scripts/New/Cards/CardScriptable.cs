using UnityEngine;

[CreateAssetMenu()]
public class CardScriptable : ScriptableObject
{
    [SerializeField] private string monsterId;

    [SerializeField] private Difficult difficult;

    [SerializeField] private Sprite monsterSprite;
    [SerializeField] private Sprite cardSprite;

    [SerializeField] private string[] ingredients;

    public string MonsterId { get { return monsterId; } }
    public Difficult Difficult { get { return difficult; } }
    public Sprite MonsterSprite { get { return monsterSprite; } }
    public Sprite CardSprite { get { return cardSprite; } }
    public string[] Ingredients { get { return ingredients; } }
}