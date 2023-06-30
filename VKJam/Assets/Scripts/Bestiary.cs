using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public class Bestiary : MonoBehaviour
{
    [Serializable]
    private struct Monster
    {
        public string name;
        public Sprite sprite;
        public Sprite type;
        public string description;
        public string ingredientstext;
        public string[] ingredients;
    }

    [SerializeField] private List<Monster> monsters;
    [SerializeField] private Button[] pageButtons;

    [SerializeField] private Image imageHolder;
    [SerializeField] private Image typeHolder;
    [SerializeField] private TextMeshProUGUI nameHolder;
    [SerializeField] private TextMeshProUGUI descriptionHolder;
    [SerializeField] private TextMeshProUGUI ingredientsHolder;

    [SerializeField] private Button previousMonsterButton;
    [SerializeField] private Button nextMonsterButton;


    [SerializeField] private GameObject catalougeCanvas;

    private int currentMonster;

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
            gameObject.SetActive(false);
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
            gameObject.SetActive(false);
            return;
        }

        currentMonster++;
        UpdateUIMonster();
    }

    private void UpdateUIMonster()
    {
        imageHolder.sprite = monsters[currentMonster].sprite;
        typeHolder.sprite = monsters[currentMonster].type;
        nameHolder.text = monsters[currentMonster].name;
        descriptionHolder.text = monsters[currentMonster].description;
        ingredientsHolder.text = monsters[currentMonster].ingredientstext;
    }

    private void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < monsters.Count)
        {
            catalougeCanvas.SetActive(false);
            gameObject.SetActive(true);
            currentMonster = pageIndex;
            UpdateUIMonster();
        }
    }

}