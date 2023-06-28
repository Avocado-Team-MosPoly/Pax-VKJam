using UnityEngine;

[CreateAssetMenu()]
public class CardSO : ScriptableObject
{
    [SerializeField] private string id;

    [SerializeField] private CardDifficulty difficulty;

    [SerializeField] private Texture cardTexture;
    [SerializeField] private Sprite monsterSprite;

    [SerializeField] private string[] ingredients;

    public string Id => id;
    public CardDifficulty Difficulty => difficulty;
    public Texture CardTexture => cardTexture;
    public Sprite MonsterSprite => monsterSprite;
    public string[] Ingredients => ingredients;
}