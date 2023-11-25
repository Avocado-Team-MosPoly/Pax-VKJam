using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;

public class CardCatalogueScript : MonoBehaviour
{
    [HideInInspector] public List<CardSO> Monsters = new();

    [SerializeField] private Image[] imageHolder = new Image[_cardInPage];
    [SerializeField] private TextMeshProUGUI[] nameHolder = new TextMeshProUGUI[_cardInPage];

    [SerializeField] private Button previousMonsterButton;
    [SerializeField] private Button nextMonsterButton;

    //[SerializeField] private GameObject templateCanvas;
    [SerializeField] private PackCardSO packCardSO;

    private Color _transparentColor = new(1,1,1,0);
    private Color _normalColor = new(1, 1, 1, 1);

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
                print(packCardSO.CardInPack[i].Card.Id);
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

            print(index);

            if (index < Monsters.Count)
            {
                imageHolder[i].GetComponentInChildren<Button>().gameObject.SetActive(true);
                imageHolder[i].color = _normalColor;
                imageHolder[i].sprite = Sprite.Create(Monsters[index].CardTexture, new Rect(0.0f, 0.0f, Monsters[index].CardTexture.width, Monsters[index].CardTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
                nameHolder[i].text = Monsters[index].Id;
            }
            else
            {
                imageHolder[i].GetComponentInChildren<Button>().gameObject.SetActive(false);
                imageHolder[i].color = _transparentColor;
                nameHolder[i].text = "";
            }
        }
    }

}