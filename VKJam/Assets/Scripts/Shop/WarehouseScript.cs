using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class WarehouseScript : MonoBehaviour
{
	[SerializeField] private List<Product> Products = new();
    
    [SerializeField] private CurrencyCatcher Display;
    [SerializeField]
    private GameObject Template;
    [SerializeField]
    private GameObject WhereInst;
    private CustomController Data;

    private void Awake()
    {
        CustomController.Initialization += SetCC;
    }

    public void SetCC(CustomController Target){
        Data = Target;
    }

    public void ChangeSection(int ToWhat)
    {
        ChangeSection((ShopFilters)ToWhat);
    }
    public void ChangeSection(ShopFilters ToWhat)
    {
        GameObject temp;
        foreach(var current in Products)
        {
            Destroy(current.gameObject);
        }
        Products.Clear();
        foreach(var current in Data.Categories[(int)ToWhat].products)
        {
            temp = Instantiate(Template, WhereInst.transform);
            Product InWork = temp.GetComponent<Product>();
            Products.Add(InWork);
            InWork.SetData(current);
        }
    }
    public void Drop()
    {
        foreach (var current in Products)
        {
            Destroy(current.gameObject);
        }
        Products.Clear();
    }
   




    private void SortListByName()
	{
		Products.Sort((N1, N2) => N1.Name.text.CompareTo(N2.Name.text));
	}

}