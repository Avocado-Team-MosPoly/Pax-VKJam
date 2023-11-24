using UnityEngine;
using System.Collections;

public class CustomController : TaskExecutor<CustomController>
{
    [SerializeField] private int FreeIndex;
    public storeSection[] Categories = new storeSection[System.Enum.GetNames(typeof(ShopFilters)).Length];
    public WareData[] Custom = new WareData[System.Enum.GetNames(typeof(ItemType)).Length];

    private void Start()
    {
        //if (Php_Connect.PHPisOnline) FetchAllProductData();
        Renumerator();
    }
    [ContextMenu("Renumarate all WareData")]
    private void Renumerator()
    {
        for (int i = 0; i < Categories.Length; i++)
        {
            var category = Categories[i];
            for (int j = 0; j < category.products.Count; j++)
            {
                var subcurrent = category.products[j];
                    if (subcurrent.Model != null)
                    {
                        subcurrent.Data.productName = subcurrent.Model.name;
                        subcurrent.Data.productCode = FreeIndex++;
                        subcurrent.Data.productPrice = 10;
                    }
            }
        }
    }
    private int CountPreloaded()
    {
        int CountData = 0;
        foreach (var current in Categories)
        {
            CountData += current.products.Count;
        }
        return CountData-12;
    }
    private void FetchAllProductData()
    {
        int count = Php_Connect.Request_DesignCount();
        int preload = CountPreloaded();
        for (int i = preload; i < count; ++i)
        {
            WareData output = new();
            output.Data.productCode = i;
            output.Request();
            Categories[(int)Categorize(output.Data.Type)].Add(output);
        }
    }
    public static ShopFilters Categorize(ItemType data)
    {
        switch (data)
        {
            case ItemType.PackCard:
                return ShopFilters.PackCard;
            case ItemType.Watch:
                return ShopFilters.Watch;
            case ItemType.Token:
                return ShopFilters.Token;
            case ItemType.LeftItem:
                return ShopFilters.LeftItem;
            case ItemType.RightItem:
                return ShopFilters.RightItem;
            case ItemType.Cauldron:
                return ShopFilters.Cauldron;
            case ItemType.Notebook:
                return ShopFilters.Books;
            case ItemType.Bestiary:
                return ShopFilters.Books;
            case ItemType.CardShirts:
                return ShopFilters.CardShirts;
            case ItemType.UI_Notebook:
                return ShopFilters.UI;
            case ItemType.UI_Bestiary:
                return ShopFilters.UI;
            case ItemType.Arm:
                return ShopFilters.UI;
            case ItemType.Mirror:
                return ShopFilters.UI;
            case ItemType.Eyes:
                return ShopFilters.UI;
            case ItemType.UI_Eraser:
                return ShopFilters.Paint;
            case ItemType.UI_Painter:
                return ShopFilters.Paint;
            case ItemType.DrawingColors:
                return ShopFilters.Paint;
            case ItemType.Avatars:
                return ShopFilters.AvatarsAndChat;
            case ItemType.AvatarFrame:
                return ShopFilters.AvatarsAndChat;
            case ItemType.Currency:
                return ShopFilters.Currency;
            default:
                return 0;
        }
    }
}
