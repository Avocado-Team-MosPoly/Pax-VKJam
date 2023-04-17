[System.Serializable]
public struct WareHouseData
{
    public int productCode;
    public string productName;
    public int productAmount;
    public int workshopNumber;
    public int productPricePerOneUnit;
    public UnityEngine.Sprite icon;

    public WareHouseData(int productCode, string productName, int productAmount, int workshopNumber, int productPricePerOneUnit, UnityEngine.Sprite icon)
    {
        this.productCode = productCode;
        this.productName = productName;
        this.productAmount = productAmount;
        this.workshopNumber = workshopNumber;
        this.productPricePerOneUnit = productPricePerOneUnit;
        this.icon = icon;
    }

}
