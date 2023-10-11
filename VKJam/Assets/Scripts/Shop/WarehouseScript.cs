using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WarehouseScript : MonoBehaviour
{
	[SerializeField] private List<Product> Products = new List<Product>();
    [SerializeField]
    private TMPro.TMP_Text InGameValue;
    [SerializeField]
    private GameObject Template;
    
    void Awake()
    {
        if (Php_Connect.PHPisOnline) Php_Connect.Request_CurrentCurrency();
        InGameValue.text = Php_Connect.Current.IGCurrency.ToString();
        if (Php_Connect.PHPisOnline) FetchAllProductData();
    }
    private void FetchAllProductData()
    {
        int i = 0;
        WareHouseData output;
        GameObject temp;
        do
        {
            output = Php_Connect.Request_DataAboutDesign(i);
            if (output.productName != "")
            {
                temp = Instantiate(Template, transform);
                Product InWork = temp.GetComponent<Product>();
                Products.Add(InWork);
                InWork.SetData(output);
            }
            i++;
        }
        while (Products[i-1].Name.text != "");
    }






    private void SortListByName()
	{
		Products.Sort((N1, N2) => N1.Name.text.CompareTo(N2.Name.text));
	}

}