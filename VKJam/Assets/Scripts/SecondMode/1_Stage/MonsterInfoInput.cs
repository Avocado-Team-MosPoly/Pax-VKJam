using TMPro;
using UnityEngine;

public class MonsterInfoInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField descriptionInputField;
    [SerializeField] private TMP_InputField[] ingredientInputFields;

    public string MonsterName => string.IsNullOrEmpty(nameInputField.text) ? GetDefaultName() : nameInputField.text;
    public string Description => string.IsNullOrEmpty(descriptionInputField.text) ? GetDefaultDescription() : descriptionInputField.text;
    public string[] Ingredients
    {
        get
        {
            string[] ingredients = new string[ingredientInputFields.Length];
            bool hasInvalidFlag = false;

            for (int i = 0; i < ingredients.Length; i++)
            {
                if (string.IsNullOrEmpty(ingredientInputFields[i].text))
                {
                    hasInvalidFlag = true;
                    break;
                }

                ingredients[i] = ingredientInputFields[i].text;
            }

            if (hasInvalidFlag)
            {
                ingredients = GetDefaultIngredients();
            }

            return ingredients;
        }
    }

    private string GetDefaultName()
    {
        return "Name";
    }

    private string GetDefaultDescription()
    {
        return "Description";
    }

    private string[] GetDefaultIngredients()
    {
        string[] ingredients = new string[ingredientInputFields.Length];

        for (int i = 0; i < ingredients.Length; i++)
            ingredients[i] = $"Ingredient {i}";

        return ingredients;
    }
}