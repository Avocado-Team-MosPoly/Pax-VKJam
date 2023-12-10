using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.VisualScripting;
using System;

public class Bestiary : MonoBehaviour
{
    public UnityEvent OnBestiaryOpened;
    public UnityEvent OnBestiaryClosed;

    [HideInInspector] public List<CardSO> Monsters = new();

    [SerializeField] private Button[] dangerousMonstersButtons;
    [SerializeField] private Button[] murderousMonstersButtons;

    [SerializeField] private Sprite dangerousIcon;
    [SerializeField] private Sprite murderousIcon;

    [SerializeField] private Image imageHolder;
    [SerializeField] private Image typeHolder;
    [SerializeField] private TextMeshProUGUI nameHolder;
    [SerializeField] private TextMeshProUGUI descriptionHolder;
    [SerializeField] private TextMeshProUGUI ingredientsHolder;

    [SerializeField] private Button previousMonsterButton;
    [SerializeField] private Button nextMonsterButton;
    [SerializeField] private GameObject chooseMonster;

    [SerializeField] private GameObject catalougeCanvas;
    [SerializeField] private GameObject templateCanvas;

    [SerializeField] private GameObject MonsterBookmark; 
    [SerializeField] private GameObject IngridientsBookmark;
    [SerializeField] private GameObject MonstersCatalogue;
    [SerializeField] private GameObject IngridientsCatalouge;

    [SerializeField] private PackCardSO packCardSO;

    private int currentMonster;

    private void Start()
    {
        previousMonsterButton.onClick.AddListener(PreviousMoster);
        nextMonsterButton.onClick.AddListener(NextMoster);
        GameManager.Instance.OnGuessMonsterStageActivatedOnClient.AddListener(MonsterGuess);
        GameManager.Instance.OnIngredientSwitchedOnClient.AddListener(IngredientGuess);
        IngredientGuess(0);

        int dangerousMonstersCount = 0;
        int murderousMonstersCount = 0;

        foreach (Button button in dangerousMonstersButtons)
        {
            button.gameObject.SetActive(false);
        }
        foreach (Button button in murderousMonstersButtons)
        {
            button.gameObject.SetActive(false);
        }
        foreach (CardSO monster in Monsters)
        {
            int pageIndex = dangerousMonstersCount + murderousMonstersCount;
            Debug.LogError(pageIndex);
            if (monster.Difficulty == CardDifficulty.Dangerous)
            {
                dangerousMonstersButtons[dangerousMonstersCount].gameObject.SetActive(true);
                dangerousMonstersButtons[dangerousMonstersCount].GetComponentInChildren<TextMeshProUGUI>().text = monster.Id;
                dangerousMonstersButtons[dangerousMonstersCount].onClick.AddListener(() => GoToPage(pageIndex));

                dangerousMonstersCount++;
            }
            else
            {
                murderousMonstersButtons[murderousMonstersCount].gameObject.SetActive(true);
                murderousMonstersButtons[murderousMonstersCount].GetComponentInChildren<TextMeshProUGUI>().text = monster.Id;
                murderousMonstersButtons[murderousMonstersCount].onClick.AddListener(() => GoToPage(pageIndex));

                murderousMonstersCount++;
            }
        }

        Debug.Log($"[Bestiary] Loaded {dangerousMonstersCount + murderousMonstersCount} monsters");

        Initialize();
    }

    private void IngredientGuess(int arg0)
    {
        MonsterBookmark.SetActive(false);
        IngridientsBookmark.SetActive(true);
        MonstersCatalogue.SetActive(false);
        IngridientsCatalouge.SetActive(true);
    }
    private void MonsterGuess(bool arg0)
    {
        MonsterBookmark.SetActive(true);
        IngridientsBookmark.SetActive(false);
        MonstersCatalogue.SetActive(true);
        IngridientsCatalouge.SetActive(false);
    }

    public void TakePack()
    {
        Monsters.Clear();

        for (int i = 0; i < PackManager.Instance.PlayersOwnedCard[GameManager.Instance.PainterId].Count; i++)
        {
            if (PackManager.Instance.PlayersOwnedCard[GameManager.Instance.PainterId][i] == true)
            {
                Monsters.Add(packCardSO.CardInPack[i].Card);
            }
        }
    }

    public void SetPack(PackCardSO _packCardSO)
    {
        packCardSO = _packCardSO;
    }
    private void Initialize()
    {
        currentMonster = 0;
        UpdateUIMonster();
    }

    private void PreviousMoster()
    {
        if (currentMonster <= 0)
        {
            catalougeCanvas.SetActive(true);
            templateCanvas.SetActive(false);
            return;
        }

        currentMonster--;
        UpdateUIMonster();
    }

    private void NextMoster()
    {
        if (currentMonster >= Monsters.Count - 1)
        {
            catalougeCanvas.SetActive(true);
            templateCanvas.SetActive(false);
            return;
        }

        currentMonster++;
        UpdateUIMonster();
    }

    private void UpdateUIMonster()
    {
        imageHolder.sprite = Monsters[currentMonster].MonsterInBestiarySprite;
        typeHolder.sprite = (Monsters[currentMonster].Difficulty == CardDifficulty.Dangerous ? dangerousIcon : murderousIcon);
        nameHolder.text = Monsters[currentMonster].Id;

        if (GameManager.Instance.Stage == Stage.MonsterGuess)
        {
            chooseMonster.SetActive(true);
            ChooseFromBook chooseFromBook = chooseMonster.GetComponent<ChooseFromBook>();
            chooseFromBook.GuessedMonster = Monsters[currentMonster].Id;
            chooseFromBook.MonsterId = currentMonster;
        }
        else
        {
            chooseMonster.SetActive(false);
        }

        descriptionHolder.text = Monsters[currentMonster].Description;
        ingredientsHolder.text = Monsters[currentMonster].GetIngredientsAsString();
    }

    private void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < Monsters.Count)
        {
            catalougeCanvas.SetActive(false);
            templateCanvas.SetActive(true);
            currentMonster = pageIndex;
            UpdateUIMonster();
        }
    }

    public void Open()
    {
        OnBestiaryOpened?.Invoke();
    }

    public void Close()
    {
        OnBestiaryClosed?.Invoke();
    }

}