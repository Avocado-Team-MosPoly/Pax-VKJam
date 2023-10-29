using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class BestiaryIngridients : MonoBehaviour
{
    [SerializeField] private int lastShownIngridient=0;
    [SerializeField] private RectTransform ingredientListContainer1;
    [SerializeField] private RectTransform ingredientListContainer2;
    [SerializeField] private GameObject ingredientInfoTemplate;
    [SerializeField] private List<string> IngridientName;
    [SerializeField] private List<Sprite> IngredientImage;

    public void UpdateIngredientList(bool up)
    {
        foreach (Transform child in ingredientListContainer1)
            Destroy(child.gameObject);
        foreach (Transform child in ingredientListContainer2)
            Destroy(child.gameObject);
        if (up)
        {
            UpdateIngredientListUp(ingredientListContainer1);
            UpdateIngredientListUp(ingredientListContainer2);
        }
        else
        {
            UpdateIngredientListDown(ingredientListContainer1);
            UpdateIngredientListDown(ingredientListContainer2);
        }
        
    }
    private void UpdateIngredientListUp(RectTransform ingredientListContainer)
    {
        int i = lastShownIngridient;
        for (; i < 10 + lastShownIngridient; i++)
        {
            if (i + lastShownIngridient >= IngridientName.Count)
            {
                break;
            }
            ;
            GameObject ingredientSingleTransform = Instantiate(ingredientInfoTemplate, ingredientListContainer);
            ingredientSingleTransform.SetActive(true);

            IngredientInfo ingredientInfoUI = ingredientSingleTransform.GetComponent<IngredientInfo>();
            ingredientInfoUI.SetIngridient(IngridientName[i + lastShownIngridient], IngredientImage[i + lastShownIngridient]);
        }
        lastShownIngridient = i;
    }
    private void UpdateIngredientListDown(RectTransform ingredientListContainer)
    {
        int i = lastShownIngridient;
        for (; i > lastShownIngridient-10; i--)
        {
            if (lastShownIngridient-i <= 0)
            {
                break;
            }
            GameObject ingredientSingleTransform = Instantiate(ingredientInfoTemplate, ingredientListContainer);
            ingredientSingleTransform.SetActive(true);

            IngredientInfo ingredientInfoUI = ingredientSingleTransform.GetComponent<IngredientInfo>();
            ingredientInfoUI.SetIngridient(IngridientName[lastShownIngridient - i], IngredientImage[lastShownIngridient - i]);
        }
        lastShownIngridient = i;
    }
}
