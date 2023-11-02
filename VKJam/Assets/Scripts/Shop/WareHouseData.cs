using System;
using UnityEngine;
[Serializable]
public class WareHouseData : ScriptableObject
{
    public int productCode;
    public string productName;
    public int productPrice;
    public UnityEngine.Sprite icon;
    public bool IsDonateVault; 
    public WareHouseData(UnityEngine.Sprite icon)
    {
        this.productCode = 0;
        this.productName = "";
        this.productPrice = 0;
        this.icon = icon;
        this.IsDonateVault = false;
    }
    public WareHouseData(int productCode, string productName, int productPrice, UnityEngine.Sprite icon, bool IsDonateVault)
    {
        this.productCode = productCode;
        this.productName = productName;
        this.productPrice = productPrice;
        this.icon = icon;
        this.IsDonateVault = IsDonateVault;
    }
}