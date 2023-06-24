using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;

public class Bestiary : MonoBehaviour
{
    [Serializable]
    private struct Monster
    {
        public string name;
        public Sprite sprite;
        public string description;
        public string[] ingredients;
    }

    [SerializeField] private List<Monster> monsters;
    
    [SerializeField] private Image imageHolder;
    [SerializeField] private TextMeshProUGUI nameHolder;
    [SerializeField] private TextMeshProUGUI descriptionHolder;

    [SerializeField] private Button previousMonsterButton;
    [SerializeField] private Button nextMonsterButton;

    private int currentMonster;

    private void Start()
    {
        previousMonsterButton.onClick.AddListener(PreviousMoster);
        nextMonsterButton.onClick.AddListener(NextMoster);

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
            return;

        currentMonster--;
        UpdateUIMonster();
    }

    private void NextMoster()
    {
        if (currentMonster >= monsters.Count - 1)
            return;

        currentMonster++;
        UpdateUIMonster();
    }

    private void UpdateUIMonster()
    {
        imageHolder.sprite = monsters[currentMonster].sprite;
        nameHolder.text = monsters[currentMonster].name;
        descriptionHolder.text = monsters[currentMonster].description;
    }

    private void Commit()
    {

    }
}