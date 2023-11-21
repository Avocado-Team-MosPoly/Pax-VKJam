using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileCustom : TaskExecutor<ProfileCustom>
{
    [SerializeField] private List<Product> Products = new();
    public SwitchModule[] Custom = new SwitchModule[System.Enum.GetNames(typeof(ItemType)).Length];
    [SerializeField]
    private GameObject Template;
    [SerializeField]
    private GameObject WhereInst;
    [SerializeField] private CustomController Data;
    [SerializeField]
    private GameObject WhatActiv;
    private void Awake()
    {
        Data = CustomController._executor;
    }
    public static void ProductChoosen(Product Target)
    {
        Debug.Log(1);
        _executor._productChoosen(Target);
    }
    private void _productChoosen(Product Target)
    {
        Debug.Log(2);
        Custom[(int)Target.Data.Data.Type].SwitchItem(Target.Data);
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
        GameObject temp;
        Drop();
        WhatActiv.SetActive(true);
        foreach (var current in Data.Categories[(int)CustomController.Categorize(ToWhat)].products)
        {
            if (current.Data.Type != ToWhat) break;
            temp = Instantiate(Template, WhereInst.transform);
            Product InWork = temp.GetComponent<Product>();
            Products.Add(InWork);
            InWork.ChooseMode = true;
            InWork.SetData(current);
        }
    }
   
}
