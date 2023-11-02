using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class BestiaryIngridients : MonoBehaviour
{
    [SerializeField] private int lastShownIngridient=0;
    [SerializeField] private int firstShownIngredient;
    [SerializeField] private RectTransform ingredientListContainer1;
    [SerializeField] private RectTransform ingredientListContainer2;
    [SerializeField] private GameObject ingredientInfoTemplate;
    [SerializeField] private List<string> ingridientName= new List<string>();
    [SerializeField] private List<Ingredient> ingridientList;
    [SerializeField] private PackCardSO packCardSO;
    [SerializeField] private GameObject NextButton;
    [SerializeField] private GameObject BeforeButton;

    public void Awake()
    {
        TakePack();
        NextButton.GetComponent<Button>().onClick.AddListener(() => UpdateIngredientList(true));
        BeforeButton.GetComponent<Button>().onClick.AddListener(() => UpdateIngredientList(false));
        lastShownIngridient = 0;
        UpdateIngredientList(true);
    }
    public void TakePack()
    {
        ingridientList.Clear();
        ingridientName.Clear();
        for (int i =0; i < packCardSO.CardInPack.Length; i++)
        {
            if(packCardSO.CardInPack[i].CardIsInOwn==true)
            {
                //ingridientName.Add(packCardSO.CardInPack[i].Card.id);
                foreach (Ingredient ingridient in packCardSO.CardInPack[i].Card.IngredientsSO)
                {
                    if (ingridientList.Contains(ingridient) !=true)
                    {
                        ingridientList.Add(ingridient);
                        ingridientName.Add(ingridient.Name);
                    }                    
                }
            }
        }
    }
    public void SetPack(PackCardSO _packCardSO)
    {
        packCardSO = _packCardSO;
    }
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
            lastShownIngridient = firstShownIngredient;
            UpdateIngredientListUp(ingredientListContainer1);
            UpdateIngredientListUp(ingredientListContainer2);
        }
        CheckButton();
    }
    private void UpdateIngredientListUp(RectTransform ingredientListContainer)
    {
        int i = lastShownIngridient;
        for (; i < 10 + lastShownIngridient; i++)
        {
            if (i >= ingridientName.Count)
            {
                if(i!=0)
                {
                    i -= 1;
                }                
                Debug.Log("Break");
                Debug.Log(i);
                break;
            };            
            GameObject ingredientSingleTransform = Instantiate(ingredientInfoTemplate, ingredientListContainer);
            ingredientSingleTransform.SetActive(true);

            IngredientInfo ingredientInfoUI = ingredientSingleTransform.GetComponent<IngredientInfo>();
            ingredientInfoUI.SetIngridient(ingridientName[i]);

        }
        lastShownIngridient = i;
    }
    private void UpdateIngredientListDown(RectTransform ingredientListContainer)
    {   
        int i = lastShownIngridient;
        for (; i > lastShownIngridient-10; i--)
        {
            if (i <= -1)
            {
                break;
            }
            GameObject ingredientSingleTransform = Instantiate(ingredientInfoTemplate, ingredientListContainer);
            ingredientSingleTransform.SetActive(true);

            IngredientInfo ingredientInfoUI = ingredientSingleTransform.GetComponent<IngredientInfo>();
            ingredientInfoUI.SetIngridient(ingridientName[i]);

        }
        lastShownIngridient = i;
    }
    private void CheckButton()
    {
        if (lastShownIngridient >= ingridientName.Count-1)
        {
            NextButton.SetActive(false);
        }
        else
        {
            NextButton.SetActive(true);
        }
        if (lastShownIngridient <= 20)
        {
            BeforeButton.SetActive(false);
        }
        else { BeforeButton.SetActive(true); }
    }
}
