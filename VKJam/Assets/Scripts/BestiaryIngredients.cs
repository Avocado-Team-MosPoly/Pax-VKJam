using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BestiaryIngredients : MonoBehaviour
{
    [HideInInspector] public List<Ingredient> IngredientList = new();
    //[HideInInspector] public List<string> IngredientName = new();

    [SerializeField] private CompareSystem compareSystem;
    [SerializeField] private PackCardSO packCardSO;
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

    private int lastShownIngridient;
    private int firstShownIngredient;
    private int spawnPositionIndex;

    private void Awake()
    {
        lastShownIngridient = 0;

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
        //IngredientName.Clear();

        for (int i = 0; i < PackManager.Instance.PlayersOwnedCard[GameManager.Instance.PainterId].Count; i++)
        {
            if (PackManager.Instance.PlayersOwnedCard[GameManager.Instance.PainterId][i] == true)
            {
                foreach (Ingredient ingridient in packCardSO.CardInPack[i].Card.IngredientsSO)
                {
                    if (IngredientList.Contains(ingridient) != true)
                    {
                        IngredientList.Add(ingridient);
                    }
                }
            }
        }
        IngredientList.Sort((x, y) => x.Name.CompareTo(y.Name));
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
            if (i >= IngredientList.Count)
            {
                if (i != 0)
                {
                    i -= 1;
                }

                break;
            }

            IngredientInfo ingredientInfoUI = Instantiate(ingredientInfoTemplate, ingredientListContainer);
            ingredientInfoUI.SetIngridient(IngredientList[i].Name, i, compareSystem, closeBestiaryButton);
            ingredientInfoUI.gameObject.SetActive(true);
            ingredientInfoUI.OnGuess.AddListener(OnIngredientSelected);

            ingredientsTexts.Add(ingredientInfoUI.IngridientName);
        }

        lastShownIngridient = i;
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

        Transform spawnPosition = spawnPositions[spawnPositionIndex];
        GameObject spawnedIngredientObject = Instantiate(IngredientList[ingredientIndex].Model, spawnPosition.position, Quaternion.identity, spawnPosition);

        if (spawnedIngredientObject == null)
            throw new System.NullReferenceException($"Ingredient prefab is null ({IngredientList[ingredientIndex].Name} ingredient)");

        spawnedIngredientObjects.Add(spawnedIngredientObject);
        isSpawnedSelectedIngredient = true;
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