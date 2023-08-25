using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class Bestiary : MonoBehaviour
{
    [SerializeField] private List<CardSO> monsters;
    [SerializeField] private Button[] pageButtons;

    [SerializeField] private Sprite dangerousIcon;
    [SerializeField] private Sprite murderousIcon;

    [SerializeField] private Image imageHolder;
    [SerializeField] private Image typeHolder;
    [SerializeField] private TextMeshProUGUI nameHolder;
    [SerializeField] private TextMeshProUGUI descriptionHolder;
    [SerializeField] private TextMeshProUGUI ingredientsHolder;

    [SerializeField] private Button previousMonsterButton;
    [SerializeField] private Button nextMonsterButton;

    [SerializeField] private GameObject catalougeCanvas;
    [SerializeField] private GameObject templateCanvas;

    private int currentMonster;

    private void OnEnable()
    {
        GameManager.Instance.SetGuesserUIActive(false);
    }

    private void OnDisable()
    {
        GameManager.Instance.SetGuesserUIActive(true);
    }

    private void Awake()
    {
        previousMonsterButton.onClick.AddListener(PreviousMoster);
        nextMonsterButton.onClick.AddListener(NextMoster);

        for (int i = 0; i < pageButtons.Length; i++)
        {
            int pageIndex = i;
            pageButtons[i].onClick.AddListener(() => GoToPage(pageIndex));
        }

        Initialize();
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
        if (currentMonster >= monsters.Count - 1)
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
        imageHolder.sprite = monsters[currentMonster].MonsterInBestiarySprite;
        typeHolder.sprite = (monsters[currentMonster].Difficulty == CardDifficulty.Dangerous ? dangerousIcon : murderousIcon);
        nameHolder.text = monsters[currentMonster].Id;
        descriptionHolder.text = monsters[currentMonster].Description;
        ingredientsHolder.text = monsters[currentMonster].GetIngredientsAsString();
    }

    private void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < monsters.Count)
        {
            catalougeCanvas.SetActive(false);
            templateCanvas.SetActive(true);
            currentMonster = pageIndex;
            UpdateUIMonster();
        }
    }
}