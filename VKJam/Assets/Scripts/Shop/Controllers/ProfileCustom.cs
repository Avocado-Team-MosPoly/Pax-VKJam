﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProfileCustom : TaskExecutor<ProfileCustom>
{
    public SwitchModule[] Custom = new SwitchModule[System.Enum.GetNames(typeof(ItemType)).Length];

    [SerializeField] private List<Product> Products = new();
    [SerializeField] private GameObject Template;
    [SerializeField] private GameObject WhereInst;
    [SerializeField] private CustomController Data;
    [SerializeField] private GameObject WhatActiv;
    [SerializeField] private ScrollRect customScrollRect;

    [SerializeField] private TMPro.TextMeshProUGUI TextType;

    private void Start()
    {
        Data = CustomController.Executor;
    }
    public static void ProductChoosen(Product Target)
    {
        //Logger.Instance.LogError(this, "name : " + Target.Data.Data.productName + "; model : " + Target.Data.Model);
        Executor.Custom[(int)Target.Data.Data.Type].SwitchItem(Target.Data);

        CustomController.Executor.Save(Target.Data);
    }

    public void Drop()
    {
        WhatActiv.SetActive(false);
        foreach (var current in Products)
        {
            Destroy(current.gameObject);
        }
        Products.Clear();
    }
    public void ChangeSection(int ToWhat)
    {
        ChangeSection((ItemType)ToWhat);
    }
    public void ChangeSection(ItemType ToWhat)
    {
        Drop();
        WhatActiv.SetActive(true);
        customScrollRect.verticalNormalizedPosition = 0f;
        TextType.text = Naming(ToWhat);

        UnityAction onClick = () => BackgroundMusic.Instance.GetComponentInChildren<SoundList>().Play("button-click");

        foreach (var current in Data.Categories[(int)CustomController.Categorize(ToWhat)].products)
        {
            if (current.Data.Type != ToWhat || !current.Data.InOwn)
                continue;

            //Logger.Instance.LogError(this, "name: " + current.Data.productName);
            //Logger.Instance.Log(this, "name: " + current.Model);

            Product InWork = Instantiate(Template, WhereInst.transform).GetComponent<Product>();
            InWork.ChooseMode = true;
            Products.Add(InWork);
            InWork.SetData(current);
            
            if (InWork.Button.interactable)
                InWork.Button.onClick.AddListener(onClick);
        }
    }
    public static string Naming(ItemType data)
    {
        switch (data)
        {
            case ItemType.PackCard:
                return "";
            case ItemType.Watch:
                return "Часы";
            case ItemType.Token:
                return "Жетоны";
            case ItemType.LeftItem:
                return "Левый предмет";
            case ItemType.RightItem:
                return "Правый предмет";
            case ItemType.Cauldron:
                return "Котел";
            case ItemType.Notebook:
                return "Блокнот";
            case ItemType.Bestiary:
                return "Бестиарий";
            case ItemType.CardShirts:
                return "Рубашка карт";
            case ItemType.UI_Notebook:
                return "Блокнот";
            case ItemType.UI_Bestiary:
                return "Бестиарий";
            case ItemType.Arm:
                return "Рука";
            case ItemType.Mirror:
                return "Зеркало";
            case ItemType.Eyes:
                return "Глаза";
            case ItemType.UI_Eraser:
                return "Ластик";
            case ItemType.UI_Painter:
                return "Мел";
            case ItemType.DrawingColors:
                return "Цвет";
            case ItemType.Avatars:
                return "Аватар";
            case ItemType.AvatarFrame:
                return "Рамка";
            case ItemType.Currency:
                return "Валюта";
            case ItemType.Table:
                return "Стол";
            default:
                return "";
        }
    }
}
