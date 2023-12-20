using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardCatalogueScript : MonoBehaviour
{
    [HideInInspector] public List<CardSystem> Monsters = new();

    [SerializeField] private Image[] imageHolder = new Image[_cardInPage];
    [SerializeField] private TextMeshProUGUI[] nameHolder = new TextMeshProUGUI[_cardInPage];

    [SerializeField] private Button previousMonsterButton;
    [SerializeField] private Button nextMonsterButton;

    [SerializeField] private string buyText;
    [SerializeField] private string inOwnText;

    //[SerializeField] private GameObject templateCanvas;

    [SerializeField] private SoundList soundList;

    private Button[] imageHolderButtons = new Button[_cardInPage];
    
    private Color _transparentColor = new(1,1,1,0);
    private Color _normalColor = new(1, 1, 1, 1);

    private int _currentPage;
    private const int _cardInPage = 8;

    private int _bufferIdCard;

    private void Start()
    {
        TakePack();
        previousMonsterButton.onClick.AddListener(PreviousPage);
        nextMonsterButton.onClick.AddListener(NextPage);

        Initialize();
        soundList.Play("Turning the page");
    }

    public void SetBuffer(int cardDBIndex)
    {
        _bufferIdCard = cardDBIndex;
    }

    public void TakePack()
    {
        Monsters.Clear();

        for (int i = 0; i < PackManager.Instance.Active.CardInPack.Length; i++)
        {
            //if (packCardSO.CardInPack[i].CardIsInOwn == true)
            print(PackManager.Instance.Active.CardInPack[i].Card.Id);
            Monsters.Add(PackManager.Instance.Active.CardInPack[i]);
        }
    }
    public void BuyCard(bool ForThePieces)
    {
        Logger.Instance.Log(this, "Trying buy card by id : " + _bufferIdCard);

        Action<string> successRequest = (string response) =>
        {
            if (response == "success")
            {
                Logger.Instance.Log(this, "Successfully purchased card by id : " + _bufferIdCard);

                CardSystem temp = PackManager.Instance.Active.SearchCardSystemById(_bufferIdCard);
                temp.CardIsInOwn = true;

                CurrencyCatcher.Executor.Refresh();
                UpdateUIPage();
            }
            else
            {
                NotificationSystem.Instance.SendLocal("Недостаточно валюты");
            }
        };
        //Action unsuccessRequest = () => { };

        StartCoroutine(Php_Connect.Request_CraftCardTry(_bufferIdCard, ForThePieces, successRequest, null));
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
        soundList.Play("Turning the page");
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
        soundList.Play("Turning the page");
    }

    private void UpdateUIPage()
    {
        int startIndex = _currentPage * _cardInPage;

        for (int i = 0; i < _cardInPage; i++)
        {
            int index = startIndex + i;

            if (imageHolderButtons[i] == null)
                imageHolderButtons[i] = imageHolder[i].GetComponentInChildren<Button>();

            if (index < Monsters.Count)
            {
                imageHolderButtons[i].gameObject.SetActive(true);
                imageHolderButtons[i].interactable = !Monsters[index].CardIsInOwn;
                imageHolderButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = Monsters[index].CardIsInOwn ? inOwnText : buyText;
                imageHolderButtons[i].onClick.RemoveAllListeners();
                imageHolderButtons[i].onClick.AddListener(() =>
                {
                    SetBuffer(Monsters[index].CardDBIndex);
                });

                imageHolder[i].color = _normalColor;
                imageHolder[i].sprite = Sprite.Create(Monsters[index].Card.CardTexture,
                    new Rect(0.0f, 0.0f, Monsters[index].Card.CardTexture.width, Monsters[index].Card.CardTexture.height),
                    new Vector2(0.5f, 0.5f), 100.0f);
                nameHolder[i].text = Monsters[index].Card.Id;
            }
            else
            {
                imageHolderButtons[i].gameObject.SetActive(false);
                imageHolder[i].color = _transparentColor;
                nameHolder[i].text = "";
            }
        }
    }
}