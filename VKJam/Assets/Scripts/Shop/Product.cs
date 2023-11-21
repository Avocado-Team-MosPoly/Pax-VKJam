using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class Product : MonoBehaviour
{
    [SerializeField] private Image Picture;

    public TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI Price;
    public WareData Data;
    [SerializeField] private Button BT;
    public bool ChooseMode;
    public void SetData(WareData NewData)
    {
        Data = NewData;
        Refresh();
    }
    private void Refresh()
    {
        Picture.sprite = Data.icon;
        //SystemName = Data.productName;
        Name.text = Data.Data.productName;
        Price.text = "X" + Data.Data.productPrice.ToString();
        BT.interactable = !Data.Data.InOwn || ChooseMode;
    }

    public void Interact()
    {
        Debug.Log("a");
        if (!Data.Data.InOwn && !Data.IsNonBuyable) StartCoroutine(Php_Connect.Request_BuyTry(Data.Data.productCode));
        else ProfileCustom.ProductChoosen(this);
    }

    public void TestButtonClick()
    {
        Debug.Log("Button Clicked");
    }
}
