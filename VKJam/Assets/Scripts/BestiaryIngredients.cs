using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BestiaryIngredients : MonoBehaviour
{
    public List<Ingredient> IngredientList = new();

    [SerializeField] private FirstModeGuessSystem compareSystem;
    [SerializeField] private IngredientInfo ingredientInfoTemplate;
    [SerializeField] private RectTransform leftIngredientsPage;
    [SerializeField] private RectTransform rightIngredientsPage;

    [SerializeField] private GameObject NextButton;
    [SerializeField] private GameObject BeforeButton;

    [SerializeField] private TextHoverEffect textHoverEffect;

    [SerializeField] private Transform[] spawnPositions;
    [SerializeField] private Button closeBestiaryButton;

    private List<TMP_Text> ingredientsTexts = new();
    private List<GameObject> spawnedIngredientObjects = new();
    private bool isSpawnedSelectedIngredient;

    private static int elementsPerPage = 10;

    private int lastShownIngridient;
    private int firstShownIngredient;
    private int spawnPositionIndex;

    private void Awake()
    {
        lastShownIngridient = 0;
        firstShownIngredient = 0;

        UpdateIngredientList(true);

        NextButton.GetComponent<Button>().onClick.AddListener(() => UpdateIngredientList(true));
        BeforeButton.GetComponent<Button>().onClick.AddListener(() => UpdateIngredientList(false));
    }

    private void Start()
    {
        GameManager.Instance.OnIngredientSwitchedOnClient.AddListener((int ingredientIndex) =>
        {
            isSpawnedSelectedIngredient = false;
            spawnPositionIndex++;

        });
        GameManager.Instance.OnRoundStartedOnClient.AddListener(() =>
        {
            foreach (GameObject spawnedIngredient in spawnedIngredientObjects)
            {
                Destroy(spawnedIngredient);
            }

            spawnedIngredientObjects.Clear();
        });
        spawnPositionIndex = 0;
        TakePack();
    }

    public void TakePack()
    {
        IngredientList.Clear();

        for (int i = 0; i < PackManager.Instance.PlayersOwnedCard[GameManager.Instance.PainterId].Count; i++)
        {
            if (PackManager.Instance.PlayersOwnedCard[GameManager.Instance.PainterId][i] == true)
            {
                if (PackManager.Instance.Active.CardInPack[i].Card is CardSO cardSO)
                {
                    foreach (Ingredient ingridient in cardSO.IngredientsSO)
                        if (IngredientList.Contains(ingridient) != true)
                            IngredientList.Add(ingridient);
                }
                else
                {
                    Logger.Instance.LogError(this, $"Invalid type of card. Must be {nameof(CardSO)}");
                    return;
                }
            }
        }
        IngredientList.Sort((x, y) => x.Name.CompareTo(y.Name));
    }
    public void UpdateIngredientList(bool up) // TODO: rework. use object pool instead of destroy and instantiate
    {
        foreach (Transform child in leftIngredientsPage)
            Destroy(child.gameObject);
        foreach (Transform child in rightIngredientsPage)
            Destroy(child.gameObject);

        ingredientsTexts.Clear();

        if (up)
        {
            firstShownIngredient = lastShownIngridient;
            lastShownIngridient = lastShownIngridient + elementsPerPage;
            UpdateIngredientList(leftIngredientsPage, firstShownIngredient, lastShownIngridient);
            firstShownIngredient = lastShownIngridient;
            lastShownIngridient = lastShownIngridient + elementsPerPage;
            UpdateIngredientList(rightIngredientsPage, firstShownIngredient, lastShownIngridient);
        }
        else
        {
            firstShownIngredient -= elementsPerPage * 3;
            lastShownIngridient = firstShownIngredient + elementsPerPage;
            UpdateIngredientList(leftIngredientsPage, firstShownIngredient, lastShownIngridient);
            firstShownIngredient = lastShownIngridient;
            lastShownIngridient += elementsPerPage;
            UpdateIngredientList(rightIngredientsPage, firstShownIngredient, lastShownIngridient);
            
        }

        textHoverEffect.SetTexts(ingredientsTexts);

        CheckButton();
    }

    private void UpdateIngredientList(RectTransform ingredientListContainer, int startIndex, int lastIndex)
    {
        if(startIndex >= lastIndex)
        {
            Debug.LogError("Start index > last index!");
            return;
        }

        for (int i = startIndex; i < lastIndex; i++)
        {
            if(i >= IngredientList.Count)
            {
                break;
            }

            IngredientInfo ingredientInfoUI = Instantiate(ingredientInfoTemplate, ingredientListContainer);
            ingredientInfoUI.SetIngridient(IngredientList[i].Name, i , compareSystem, closeBestiaryButton);
            ingredientInfoUI.gameObject.SetActive(true);
            ingredientInfoUI.OnGuess.AddListener(OnIngredientSelected);

            ingredientsTexts.Add(ingredientInfoUI.IngridientName);
        }

        GameManager.Instance.SoundList.Play("Turning the page");
    }

    private void OnIngredientSelected(int ingredientIndex)
    {
        
        if (isSpawnedSelectedIngredient)
        {
            Destroy(spawnedIngredientObjects[spawnedIngredientObjects.Count-1]);
            spawnedIngredientObjects.RemoveAt(spawnedIngredientObjects.Count - 1);
            isSpawnedSelectedIngredient= false;
        }

        if (spawnedIngredientObjects.Count >= spawnPositions.Length)
            return;

        if (IngredientList[ingredientIndex].Model == null)
            Logger.Instance.LogError(this, new System.NullReferenceException($"Ingredient prefab is null ({IngredientList[ingredientIndex].Name} ingredient)"));
        else
        {
            Transform spawnTransform = spawnPositions[spawnPositionIndex];
            GameObject spawnedIngredientObject = Instantiate(IngredientList[ingredientIndex].Model, spawnTransform);
            spawnedIngredientObjects.Add(spawnedIngredientObject);
            isSpawnedSelectedIngredient = true;
        }

        GameManager.Instance.SoundList.Play("Choose ingredient");
    }

    private void CheckButton()
    {
        if (lastShownIngridient >= IngredientList.Count - 1)
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

    public int GetIngredientIndexById(string ingredientId)
    {
        if (string.IsNullOrEmpty(ingredientId))
            Debug.LogWarning($"[{this.name}] Argument ingredientId is null or empty");
        else if (IngredientList == null || IngredientList.Count <= 0)
            Debug.LogWarning($"[{this.name}] IngredientList is null or empty");
        else
        {
            for (int i = 0; i < IngredientList.Count; i++)
            {
                if (IngredientList[i].Name == ingredientId)
                    return i;
            }
        }

        return -1;
    }
}