[System.Serializable]
public struct WareHouseData
{
    public int productCode;
    public string productName;
    public int productPrice;
    public UnityEngine.Sprite icon;
    public bool IsDonateVault;

    public WareHouseData(int productCode, string productName, int productPrice, UnityEngine.Sprite icon, bool IsDonateVault)
    {
        this.productCode = productCode;
        this.productName = productName;
        this.productPrice = productPrice;
        this.icon = icon;
        this.IsDonateVault = IsDonateVault;
    }


}
