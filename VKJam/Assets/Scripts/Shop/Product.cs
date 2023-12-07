using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Product : MonoBehaviour
{
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
        Data = NewData;
        Refresh();
    }
    private void Refresh()
    {
        if (Data.Data.Type == ItemType.AvatarFrame) Picture.gameObject.transform.localScale *= 1.4f;
        Picture.sprite = Data.icon;
        //transform.localScale *= (int)ItemScale/100;
        //SystemName = Data.productName;
        Name.text = Data.Data.productName;
        Price.text = !ChooseMode ? "X" + Data.Data.productPrice.ToString() : "";
        if (!ChooseMode) DisplayTypeCurrency.sprite = Data.Data.IsDonateVault ? DonatCurrency : InGameCurrency;
        else DisplayTypeCurrency.gameObject.SetActive(false);
        BT.interactable = !Data.Data.InOwn || ChooseMode;
    }
    public void RemoveFromWarehouse()
    {
        WarehouseScript._executor.RemoveProduct(this);
        Destroy(gameObject);
    }

    public void Interact()
    {
        if (!Data.Data.InOwn && !Data.IsNonBuyable) 
        {
            if(Php_Connect.PHPisOnline) { 
                string res = Php_Connect.Request_BuyTry(Data.Data.productCode);
                if (res == "success")
                {
                    Data.Data.InOwn = true;
                    RemoveFromWarehouse();
                }
            }
            else
            {
                if (Data.Data.IsDonateVault) Php_Connect.Current.DCurrency -= Data.Data.productPrice;
                else Php_Connect.Current.IGCurrency -= Data.Data.productPrice;
                Data.Data.InOwn = true;
                RemoveFromWarehouse();
            }
        }
                
        else ProfileCustom.ProductChoosen(this);
    }

}
