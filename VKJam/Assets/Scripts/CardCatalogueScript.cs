using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CardCatalogueScript : MonoBehaviour
{
    [HideInInspector] public List<CardSO> Monsters = new();

    [SerializeField] private Image[] imageHolder = new Image[_cardInPage];
    [SerializeField] private TextMeshProUGUI[] nameHolder = new TextMeshProUGUI[_cardInPage];

    [SerializeField] private Button previousMonsterButton;
    [SerializeField] private Button nextMonsterButton;

    //[SerializeField] private GameObject templateCanvas;
    [SerializeField] private PackCardSO packCardSO;

    private int _currentPage;
    private const int _cardInPage = 8;

    private void Start()
    {
        TakePack();
        previousMonsterButton.onClick.AddListener(PreviousPage);
        nextMonsterButton.onClick.AddListener(NextPage);

        Initialize();
    }

    public void TakePack()
    {
        Monsters.Clear();

        for (int i = 0; i < packCardSO.CardInPack.Length; i++)
        {
            if (packCardSO.CardInPack[i].CardIsInOwn == true)
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
        _currentPage = 0;
        UpdateUIPage();
    }

    private void PreviousPage()
    {

        if (_currentPage <= 0)
        {
            return;
        }

        _currentPage--;
        UpdateUIPage();
    }

    private void NextPage()
    {
        int totalPages = Mathf.CeilToInt((float)Monsters.Count / _cardInPage);

        if (_currentPage >= totalPages - 1)
        {
            _currentPage = 0;
            UpdateUIPage();
            return;
        }

        _currentPage++;
        UpdateUIPage();
    }

    private void UpdateUIPage()
    {
        int startIndex = _currentPage * _cardInPage;

        for (int i = 0; i < _cardInPage; i++)
        {
            int index = startIndex + i;

            if (index < Monsters.Count)
            {
                imageHolder[i].sprite = Monsters[index].MonsterInBestiarySprite;
                nameHolder[i].text = Monsters[index].Id;
            }
            else
            {
                imageHolder[i].sprite = null;
                nameHolder[i].text = "";
            }
        }
    }

}