using UnityEngine;
using TMPro;

public class Hint : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ingredientText;

    public void SetData(string ingredientName)
    {
        Debug.Log($"New Ingredient setted ({gameObject.name}) : {ingredientName}");
        
        if (ingredientText == null)
            ingredientText = GetComponentInChildren<TextMeshProUGUI>();

        ingredientText.text = ingredientName;
    }
}