using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileCustom : MonoBehaviour
{
    [SerializeField] private List<Product> Products = new();
    public WareData[] Custom = new WareData[System.Enum.GetNames(typeof(ItemType)).Length];
    [SerializeField]
    private GameObject Template;
    [SerializeField]
    private GameObject WhereInst;
    private CustomController Data;
    [SerializeField]
    private GameObject WhatActiv;
    private void Awake()
    {
        CustomController.Initialization += SetCC;
    }

    public void SetCC(CustomController Target)
    {
        Data = Target;
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
            InWork.SetData(current);
        }
    }
   
}
