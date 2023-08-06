using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WarehouseScript : MonoBehaviour
{
	[SerializeField] private List<Product> Products = new List<Product>();
    [SerializeField]
    private GameObject Template;
    private void Awake()
    {
		//SortListByName();
	}

    void Start()
    {
        StartCoroutine(FetchAllProductData());
    }
    private IEnumerator FetchAllProductData()
    {
        int i = 0;
        WareHouseData output;
        GameObject temp;
        do
        {
            output = new WareHouseData();
            yield return StartCoroutine(Php_Connect.Request_DataAboutDesign(i, (tempOutput) => {
                output = tempOutput;
                Debug.Log(output.productName);
                if (output.productName != "") {
                    temp = Instantiate(Template, transform);
                    Product InWork = temp.GetComponent<Product>();
                    Products.Add(InWork);
                    InWork.SetData(output);
                }
                else return;
            }));
            i++;
        }
        while (Products[i-1].Name.text != "");
    }






    private void SortListByName()
	{
		Products.Sort((N1, N2) => N1.Name.text.CompareTo(N2.Name.text));
	}

}