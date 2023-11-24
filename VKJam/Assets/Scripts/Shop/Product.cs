using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Product : MonoBehaviour
{
    [SerializeField] private Image Picture;

    public TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI Price;
    public WareData Data;
    [SerializeField] private Button BT;
    public bool ChooseMode;
    public void SetData( WareData NewData)
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
        if (!Data.Data.InOwn && !Data.IsNonBuyable) 
        {
            if(Php_Connect.PHPisOnline) { 
                string res = Php_Connect.Request_BuyTry(Data.Data.productCode);
                if (res == "success") ;
            }
        }
                
        else ProfileCustom.ProductChoosen(this);
    }

}
