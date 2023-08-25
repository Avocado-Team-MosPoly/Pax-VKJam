using UnityEngine;

[CreateAssetMenu()]
public class CardSO : ScriptableObject
{
    [SerializeField] private string id;

    [SerializeField] private CardDifficulty difficulty;

    [SerializeField] private Texture cardTexture;
    [SerializeField] private Texture monsterTexture;
    [SerializeField] private Sprite monsterInBestiarySprite;

    [SerializeField] private string description;
    [SerializeField] private string[] ingredients;

    public string Id => id;
    public CardDifficulty Difficulty => difficulty;
    public Texture CardTexture => cardTexture;
    public Texture MonsterTexture => monsterTexture;
    public Sprite MonsterInBestiarySprite => monsterInBestiarySprite;

    public string Description => description;
    public string[] Ingredients => ingredients;
    
    public string GetIngredientsAsString()
    {
        string ingredietList = string.Empty;

        foreach (string ingrediet in ingredients)
            ingredietList += ingrediet + "\n";
        
        return ingredietList;
    }
}