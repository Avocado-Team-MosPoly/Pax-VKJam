using UnityEngine;
using TMPro;

public class Hint : MonoBehaviour
{
    public void SetData(string ingredientName)
    {
        Debug.Log("New Ingredient setted " + ingredientName);
        transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ingredientName;
    }
}