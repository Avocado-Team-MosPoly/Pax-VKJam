using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Product : MonoBehaviour
{
    public Button Button => BT;

    [SerializeField] private Image Picture;
    [SerializeField] private Image DisplayTypeCurrency;

    [SerializeField] private Sprite DonatCurrency;
    [SerializeField] private Sprite InGameCurrency;
    public TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI Price;
    public WareData Data;
    [SerializeField] private Button BT;
    public bool ChooseMode;

    public void SetData(WareData NewData)
    {
        //if (Data.Data.Type == ItemType.AvatarFrame)
        //{
        //    if (NewData.Data.Type != ItemType.AvatarFrame)
        //        Picture.transform.localScale *= 1.4f;
        //    else
        //        Picture.transform.localScale /= 1.4f;
        //}
        //else
        if (NewData.Data.Type == ItemType.AvatarFrame)
        {
            Picture.transform.localScale *= 1.4f;
        }

        //Logger.Instance.LogWarning(this, "Type : " + NewData.Data.Type.ToString());
        //Logger.Instance.Log(this, "Name : " + NewData.Data.productName);
        //Logger.Instance.Log(this, "Model : " + NewData.Model);

        Data = NewData;
        Refresh();
    }
    private void Refresh()
    {
        Picture.sprite = Data.icon;
        //transform.localScale *= (int)ItemScale/100;
        //SystemName = Data.productName;
        Name.text = Data.Data.productName;
        Price.text = !ChooseMode ? "X" + Data.Data.productPrice.ToString() : "";

        if (!ChooseMode)
            DisplayTypeCurrency.sprite = !Data.Data.IsDonateVault ? DonatCurrency : InGameCurrency;
        else
            DisplayTypeCurrency.gameObject.SetActive(false);

        bool canBuy = !Data.Data.InOwn || ChooseMode;
        BT.interactable = canBuy;
    }
    public void RemoveFromWarehouse()
    {
        WarehouseScript._executor.RemoveProduct(this);
        //Destroy(gameObject);
    }

    public void Interact()
    {
        if (!Data.Data.InOwn && !Data.IsNonBuyable)
        {
            if (Php_Connect.PHPisOnline)
            {
                Action successRequest = () =>
                {
                    Data.Data.InOwn = true;
                    RemoveFromWarehouse();

                    CurrencyCatcher._executor.Refresh();
                };

                StartCoroutine(Php_Connect.Request_BuyTry(Data.Data.productCode, successRequest, null));
            }
            else
            {
                if (Data.Data.IsDonateVault) Php_Connect.Current.DCurrency -= Data.Data.productPrice;
                else Php_Connect.Current.IGCurrency -= Data.Data.productPrice;
                Data.Data.InOwn = true;
                RemoveFromWarehouse();
            }
        }

        else
        {
            //Logger.Instance.LogError(this, "name : " + Data.Data.productName + "; model : " + Data.Model);
            ProfileCustom.ProductChoosen(this);
        }
    }
}