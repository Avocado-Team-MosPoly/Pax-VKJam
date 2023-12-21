using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class Bestiary : MonoBehaviour
{
    public UnityEvent OnBestiaryOpened;
    public UnityEvent OnBestiaryClosed;

    public List<CardSO> Monsters = new();
    public List<CardSO> sortedMonsters = new();

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

    //[SerializeField] private GameObject MonsterBookmark; 
    //[SerializeField] private GameObject IngridientsBookmark;
    [SerializeField] private GameObject MonstersCatalogue;
    [SerializeField] private GameObject IngridientsCatalouge;

    private int currentMonster;

    private int choosenMonster;

    private void Start()
    {
        previousMonsterButton.onClick.AddListener(PreviousMoster);
        nextMonsterButton.onClick.AddListener(NextMoster);
        GameManager.Instance.OnGuessMonsterStageActivatedOnClient.AddListener(MonsterGuess);
        GameManager.Instance.OnRoundStartedOnClient.AddListener(IngredientGuess);
        IngredientGuess();

        int dangerousMonstersCount = 0;
        int murderousMonstersCount = 0;

        TakePack();
        foreach (Button button in dangerousMonstersButtons)
        {
            button.gameObject.SetActive(false);
        }
        foreach (Button button in murderousMonstersButtons)
        {
            button.gameObject.SetActive(false);
        }

        for(int i = 0; i < Monsters.Count; i++)
        {
            Button temp;
            var index = i;

            if (Monsters[index].Difficulty == CardDifficulty.Dangerous)
            {
                temp = dangerousMonstersButtons[dangerousMonstersCount];

                temp.gameObject.SetActive(true);
                temp.GetComponentInChildren<TextMeshProUGUI>().text = Monsters[index].Id;
                temp.onClick.RemoveAllListeners();
                temp.onClick.AddListener(() => GoToPage(index));
                
                dangerousMonstersCount++;
            }
            else
            {
                temp = murderousMonstersButtons[murderousMonstersCount];

                temp.gameObject.SetActive(true);
                temp.GetComponentInChildren<TextMeshProUGUI>().text = Monsters[index].Id;
                temp.onClick.RemoveAllListeners();
                temp.onClick.AddListener(() => GoToPage(index));

                murderousMonstersCount++;
            }
        }

        Initialize();
    }

    private void IngredientGuess()
    {
        //MonsterBookmark.SetActive(false);
        //IngridientsBookmark.SetActive(true);
        MonstersCatalogue.SetActive(false);
        IngridientsCatalouge.SetActive(true);
    }
    private void MonsterGuess(bool arg0)
    {
        //MonsterBookmark.SetActive(true);
        //IngridientsBookmark.SetActive(false);
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
                Monsters.Add(PackManager.Instance.Active.CardInPack[i].Card);
            }
        }

        Monsters = Monsters.OrderBy(x => (int)(x.Difficulty)).ToList();
    }

    private void Initialize()
    {
        currentMonster = 0;
        UpdateUIMonster();
    }

    private void PreviousMoster()
    {
        currentMonster--;
        UpdateUIMonster();
    }

    private void NextMoster()
    {
        currentMonster++;
        UpdateUIMonster();
    }

    private void UpdateUIMonster()
    {
        if (currentMonster < 0 || currentMonster >= Monsters.Count)
        {
            Logger.Instance.LogWarning(this, new System.ArgumentOutOfRangeException());
            return;
        }
        else if (currentMonster == 0)
        {
            nextMonsterButton.gameObject.SetActive(true);
            previousMonsterButton.gameObject.SetActive(false);
        }
        else if (currentMonster == Monsters.Count - 1)
        {
            previousMonsterButton.gameObject.SetActive(true);
            nextMonsterButton.gameObject.SetActive(false);
        }
        else
        {
            nextMonsterButton.gameObject.SetActive(true);
            previousMonsterButton.gameObject.SetActive(true);
        }

        imageHolder.sprite = Monsters[currentMonster].MonsterInBestiarySprite;
        typeHolder.sprite = (Monsters[currentMonster].Difficulty == CardDifficulty.Dangerous ? dangerousIcon : murderousIcon);
        nameHolder.text = Monsters[currentMonster].Id;

        if (GameManager.Instance.Stage == Stage.MonsterGuess)
        {
            chooseMonster.SetActive(true);
            ChooseFromBook chooseFromBook = chooseMonster.GetComponent<ChooseFromBook>();

            chooseFromBook.OnSelected += (int monsterID) => choosenMonster = monsterID;

            chooseFromBook.GuessedMonster = Monsters[currentMonster].Id;
            chooseFromBook.MonsterId = currentMonster;
            chooseFromBook.Selected = currentMonster == choosenMonster;

            chooseFromBook.Show(chooseFromBook.Selected);
        }
        else
        {
            chooseMonster.SetActive(false);
        }

        descriptionHolder.text = Monsters[currentMonster].Description;
        ingredientsHolder.text = Monsters[currentMonster].GetIngredientsAsString();

        GameManager.Instance.SoundList.Play("Turning the page");
    }

    private void GoToPage(int pageIndex)
    {
        Debug.LogWarning(pageIndex);
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