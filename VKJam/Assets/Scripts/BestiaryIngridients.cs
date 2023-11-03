using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BestiaryIngridients : MonoBehaviour
{
    [SerializeField] private CompareSystem compareSystem;
    [SerializeField] private PackCardSO packCardSO;
    [SerializeField] private IngredientInfo ingredientInfoTemplate;

    [SerializeField] private RectTransform leftIngredientsPage;
    [SerializeField] private RectTransform rightIngredientsPage;

    [SerializeField] private GameObject NextButton;
    [SerializeField] private GameObject BeforeButton;

    [SerializeField] private TextHoverEffect textHoverEffect;

    private List<string> ingridientName = new();
    private List<Ingredient> ingridientList = new();

    private List<TMP_Text> ingredientsTexts = new();

    private int lastShownIngridient;
    private int firstShownIngredient;

    public void Awake()
    {
        TakePack();

        lastShownIngridient = 0;

        UpdateIngredientList(true);

        NextButton.GetComponent<Button>().onClick.AddListener(() => UpdateIngredientList(true));
        BeforeButton.GetComponent<Button>().onClick.AddListener(() => UpdateIngredientList(false));
    }
    public void TakePack()
    {
        ingridientList.Clear();
        ingridientName.Clear();

        for (int i = 0; i < packCardSO.CardInPack.Length; i++)
        {
            if(packCardSO.CardInPack[i].CardIsInOwn==true)
            {
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
        foreach (Transform child in leftIngredientsPage)
            Destroy(child.gameObject);
        foreach (Transform child in rightIngredientsPage)
            Destroy(child.gameObject);

        ingredientsTexts.Clear();

        if (up)
        {
            UpdateIngredientListUp(leftIngredientsPage);
            UpdateIngredientListUp(rightIngredientsPage);
        }
        else
        {
            lastShownIngridient = firstShownIngredient;
            UpdateIngredientListUp(leftIngredientsPage);
            UpdateIngredientListUp(rightIngredientsPage);
        }

        textHoverEffect.SetTexts(ingredientsTexts);

        CheckButton();
    }

    private void UpdateIngredientListUp(RectTransform ingredientListContainer)
    {
        int i = lastShownIngridient;
        for (; i < 10 + lastShownIngridient; i++)
        {
            if (i >= ingridientName.Count)
            {
                if (i != 0)
                {
                    i -= 1;
                }

                break;
            }

            IngredientInfo ingredientInfoUI = Instantiate(ingredientInfoTemplate, ingredientListContainer);
            ingredientInfoUI.SetIngridient(ingridientName[i], compareSystem);
            ingredientInfoUI.gameObject.SetActive(true);

            ingredientsTexts.Add(ingredientInfoUI.IngridientName);
        }

        lastShownIngridient = i;
    }

    // why?
    //private void UpdateIngredientListDown(RectTransform ingredientListContainer)
    //{   
    //    int i = lastShownIngridient;
    //    for (; i > lastShownIngridient - 10; i--)
    //    {
    //        if (i <= -1)
    //        {
    //            break;
    //        }
    //        GameObject ingredientSingleTransform = Instantiate(ingredientInfoTemplate, ingredientListContainer);
    //        ingredientSingleTransform.SetActive(true);

    //        IngredientInfo ingredientInfoUI = ingredientSingleTransform.GetComponent<IngredientInfo>();
    //        ingredientInfoUI.SetIngridient(ingridientName[i], compareSystem);
    //    }
    //    lastShownIngridient = i;
    //}

    private void CheckButton()
    {
        if (lastShownIngridient >= ingridientName.Count - 1)
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
        else
        {
            BeforeButton.SetActive(true);
        }
    }
}