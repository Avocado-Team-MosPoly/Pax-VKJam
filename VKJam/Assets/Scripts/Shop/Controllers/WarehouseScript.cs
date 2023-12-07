using UnityEngine;
using System.Collections.Generic;

public class WarehouseScript : TaskExecutor<WarehouseScript>
{
	[SerializeField] private List<Product> Products = new();
    
    [SerializeField] private CurrencyCatcher Display;
    [SerializeField]
    private GameObject Template;
    [SerializeField]
    private GameObject WhereInst;
    [SerializeField]
    private CustomController Data;


    private void Awake()
    {
        Data = CustomController._executor;
        Denote();
    }

    public void ChangeSection(int ToWhat)
    {
        ChangeSection((ShopFilters)ToWhat);
    }
    public void ChangeSection(ShopFilters ToWhat)
    {
        GameObject temp;
        Drop();
        foreach (var current in Data.Categories[(int)ToWhat].products)
        {
            if (current.Data.InOwn || current.Data.productName == "") continue;
            temp = Instantiate(Template, WhereInst.transform);
            Product InWork = temp.GetComponent<Product>();
            InWork.ChooseMode = false;
            Products.Add(InWork);
            InWork.SetData(current);
        }
    }

    public void RemoveProduct(Product productToRemove)
    {
        Products.Remove(productToRemove);
    }
    public void Drop()
    {
        foreach (var current in Products)
        {
            Destroy(current.gameObject);
        }
        Products.Clear();
    }
  
}