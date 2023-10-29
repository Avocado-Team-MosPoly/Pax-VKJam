using UnityEngine;

[CreateAssetMenu()]
public class CardSO : ScriptableObject
{
    public string id;

    [SerializeField] private CardDifficulty difficulty;

    [SerializeField] private Texture cardTexture;
    [SerializeField] private Texture monsterTexture;
    [SerializeField] private Sprite monsterInBestiarySprite;

    [SerializeField] private string description;
    [SerializeField] private string[] ingredients;

    [SerializeField] private Ingredient[] ingredientsSO;

    public string Id => id;
    public CardDifficulty Difficulty => difficulty;
    public Texture CardTexture => cardTexture;
    public Texture MonsterTexture => monsterTexture;
    public Sprite MonsterInBestiarySprite => monsterInBestiarySprite;

    public string Description => description;
    public string[] Ingredients => ingredients;

    /*private void OnValidate()
    {
        foreach(var current in ingredientsSO)
        {
            current.addMonster(this);
        }
    }*/
    /*
    private void OnValidate()
    {
        ingredients = new string[ingredientsSO.Length];
        for(int i = 0;i< ingredientsSO.Length; ++i)
        {
            ingredients[i] = ingredientsSO[i].Name;
        }
    }*/

    public string GetIngredientsAsString()
    {
        string ingredietList = string.Empty;

        foreach (string ingrediet in ingredients)
            ingredietList += ingrediet + "\n";
        
        return ingredietList;
    }
}