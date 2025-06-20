using UnityEngine;

[CreateAssetMenu()]
public class CardSO : BaseCardInfo
{
    public string id;

    [SerializeField] private CardDifficulty difficulty;

    [SerializeField] private Texture2D cardTexture;
    [SerializeField] private Texture monsterTexture;
    [SerializeField] private Sprite monsterInBestiarySprite;

    [SerializeField] private string description;

    [SerializeField] private Ingredient[] ingredientsSO;
    public Ingredient[] IngredientsSO => ingredientsSO;
    public override string[] Ingredients
    {
        get
        {
            string[] ingredients = new string[ingredientsSO.Length];

            for (int i = 0; i < ingredients.Length; i++)
                ingredients[i] = ingredientsSO[i].Name;

            return ingredients;
        }
    }

    public override string Id => id;
    public override CardDifficulty Difficulty => difficulty;
    public override Texture2D CardTexture => cardTexture;
    public Texture MonsterTexture => monsterTexture;
    public Sprite MonsterInBestiarySprite => monsterInBestiarySprite;

    public override string Description => description;

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