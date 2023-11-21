using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class Product : MonoBehaviour
{
    [SerializeField] private Image Picture;
    
    public TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI Price;
    [SerializeField] private WareData Data;
    [SerializeField] private Button BT;

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
        BT.interactable = !Data.Data.InOwn;
    }

    public void Interact()
    {
        StartCoroutine(Php_Connect.Request_BuyTry(Data.Data.productCode));
    }
}
