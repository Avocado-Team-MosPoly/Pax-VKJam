using UnityEngine;
using TMPro;

public class Hint : MonoBehaviour
{
    public void SetHint(string text)
    {
        Debug.Log("New Ingredient setted" + text);
        
        gameObject.SetActive(true);
        transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }

    public void HideHint()
    {
        gameObject.SetActive(false);
    }
}