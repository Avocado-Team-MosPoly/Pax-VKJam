using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu()]
public class CardSO : ScriptableObject
{
    public string id;

    [SerializeField] private CardDifficulty difficulty;

    [SerializeField] private string description;

    [SerializeField] private Ingredient[] ingredientsSO;
    public Ingredient[] IngredientsSO => ingredientsSO;

    public string Id => id;
    public CardDifficulty Difficulty => difficulty;
    public Texture2D cardTexture;
    public Texture monsterTexture;
    public Sprite monsterInBestiarySprite;

    public string Description => description;

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
        string ingredientList = string.Empty;

        foreach (Ingredient ingredient in ingredientsSO)
            ingredientList += ingredient.Name + "\n";
        
        return ingredientList;
    }

    public override string ToString()
    {
        return id;
    }
}