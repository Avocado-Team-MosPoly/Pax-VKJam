using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class Product : RandomItem
{
    [SerializeField] private Image Picture;
    public TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI Price;
    [SerializeField] private WareHouseData Data;

    public void SetData(WareHouseData NewData)
    {
        Data = NewData;
        Refresh();
    }
    private void Refresh()
    {
        Picture.sprite = Data.icon;
        Name.text = Data.productName;
        Price.text = "X" + Data.productPrice.ToString();
    }

    public void BuyTry()
    {
        StartCoroutine(Php_Connect.Request_BuyTry(Data.productCode));
    }
}
