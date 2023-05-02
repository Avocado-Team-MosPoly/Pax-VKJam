[System.Serializable]
public struct WareHouseData
{
    public int productCode;
    public string productName;
    public int productPrice;
    public UnityEngine.Sprite icon;

    public WareHouseData(int productCode, string productName, int productPrice, UnityEngine.Sprite icon)
    {
        this.productCode = productCode;
        this.productName = productName;
        this.productPrice = productPrice;
        this.icon = icon;
    }

}
