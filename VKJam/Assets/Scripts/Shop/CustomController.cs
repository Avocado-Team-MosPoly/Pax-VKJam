using UnityEngine;

public class CustomController : MonoBehaviour
{
    public delegate void Init(CustomController sceneName);
    public static event Init Initialization;
    public storeSection[] Categories = new storeSection[System.Enum.GetNames(typeof(ShopFilters)).Length];
    public WareData[] Custom = new WareData[System.Enum.GetNames(typeof(ItemType)).Length];

    private void Awake()
    {
        SceneLoader.EndLoad += ChangeScene;
        if (Php_Connect.PHPisOnline) FetchAllProductData();
    }
    private void ChangeScene(string sceneName)
    {
        if (sceneName != "ProfileCastom" && sceneName != "Shop") return;
        Initialization?.Invoke(this);
    }
    private int CountPreloaded()
    {
        int CountData = 0;
        foreach (var current in Categories)
        {
            CountData += current.products.Count;
        }
        return CountData;
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
            case ItemType.AvatarsAndChat:
                return ShopFilters.AvatarsAndChat;
            case ItemType.Currency:
                return ShopFilters.Currency;
            default:
                return 0;
        }
    }
}
