using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomController : TaskExecutor<CustomController>
{
    [SerializeField] private int FreeIndex;
    public StoreSection[] Categories = new StoreSection[Enum.GetNames(typeof(ShopFilters)).Length];
    public WareData[] Custom = new WareData[Enum.GetNames(typeof(ItemType)).Length];
    public WareData[] Standart = new WareData[Enum.GetNames(typeof(ItemType)).Length];

    public IEnumerator Init()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (var section in Categories)
        {
            foreach (var ware in section.products)
            {
                //Logger.Instance.LogWarning(this, ware.Data.Type.ToString() + " " + ware.Data.productName + " : " + ware.Model);
                //yield return StartCoroutine(Php_Connect.Request_CheckOwningDesign(ware.Data.productCode, ware.OnCheckOwningDesignComplete));
            }
        }

        Action<string> OnCompleted = (string response) =>
        {
            if (response == null)
            {
                Debug.LogError("CheckOwningDesign error, response null");
                return;
            }

            // parce

            List<string> resp = new();
            resp.AddRange(response.Split('\n'));

            if(!int.TryParse(resp[0], out int i) && response != "empty")
            {
                Debug.LogError("CheckOwningDesign error");
                return;
            }

            foreach (var section in Categories)
            {
                foreach (var ware in section.products)
                {
                    if (ware.Data.productCode == 0 || resp.Contains(ware.Data.productCode.ToString())) //
                    {
                        ware.OnCheckOwningDesignComplete(true);
                    }
                    else
                    {
                        ware.OnCheckOwningDesignComplete(false);
                    }
                }
            }
        };

        yield return StartCoroutine(Php_Connect.Request_CheckOwningDesign(OnCompleted));

        Load();

        //if (Php_Connect.PHPisOnline)
        //FetchAllProductData();

        Logger.Instance.Log(this, "Initialized");
    }

    private void SetDefaults()
    {
        for (int i = 0; i < Custom.Length; i++)
            Custom[i] = Standart[i];
    }

    [ContextMenu("Upload Data into DB")]
    private IEnumerator Upload()
    {
        foreach (var section in Categories)
        {
            foreach (var ware in section.products)
            {
                if (ware.Data.productCode == 0)
                    continue;

                //Action<string> successRequest = (string response) => { };
                //Action unsuccessRequest = () => { };

                yield return StartCoroutine(Php_Connect.Request_UploadData(ware.Data, null, null));
            }
        }
    }

    private void Load()
    {
        /*foreach (ItemType cur in System.Enum.GetValues(typeof(ItemType)))
        {
            Custom[(int)cur] = Search(PlayerPrefs.GetInt("Custom_" + cur), cur);
        }*/

        Action<string> successRequest = (string response) =>
        {
            if (response == "failed")
            {
                Logger.Instance.LogWarning(this, "Unable to find saved custom on server. Setted defaults");
                SetDefaults();
                return;
            }

            string[] resp = response.Split('\n');
            foreach(var element in resp)
            {
                if(element.Length > 0)
                {
                    string[] e = element.Split(' ');

                    Custom[int.Parse(e[0])] = Search(int.Parse(e[1]), (ItemType)int.Parse(e[0]));
                }
            }
        };

        StartCoroutine(Php_Connect.Request_LoadCastom(successRequest));
    }

    public WareData Search(int id, ItemType Type)
    {
        foreach(var current in Categories[(int)Categorize(Type)].products)
        {
            if (current.Data.productCode == id && current.Data.InOwn) return current;
        }

        return Standart[(int)Type];
    }
    public WareData Search(DesignSelect Sel)
    {
        foreach (var current in Categories[(int)Categorize((ItemType)Sel.type)].products)
        {
            if (current.Data.productCode == Sel.design)
            {
                current.Data.InOwn = true;
                return current;
            }
        }
        return null;
    }
    public void Save(WareData New)
    {
        Custom[(int)New.Data.Type] = New;

        //save choosen item to db
        System.Action<string> successRequest = (string response) =>
        {
            Debug.Log($"Saved item {New.Data.productCode} with type {New.Data.Type} to DB");
        };

        StartCoroutine(Php_Connect.Request_SaveCastom(New.Data.Type, New.Data.productCode, successRequest));

        //PlayerPrefs.SetInt("Custom_" + New.Data.Type, New.Data.productCode);
        //PlayerPrefs.Save();
    }
    [ContextMenu("Denote Exist ItemType")]
    private void DenoteExist()
    {
        HashSet<ItemType> uniqueItemTypes = new HashSet<ItemType>();

        foreach (var section in Categories)
        {
            foreach (var ware in section.products)
            {
                if (ware.Data.Type == ItemType.PackCard) Debug.Log(ware.Data.productName + " " + Categories);
                uniqueItemTypes.Add(ware.Data.Type);
            }
        }
        string output = "";
        foreach (var itemType in uniqueItemTypes)
        {
            output += itemType.ToString() + "\n";
        }
        Debug.Log(output);
    }
    [ContextMenu("Fill Data in Standart")]
    private void StandartFill()
    {
        Debug.Log("Start filling");
        Standart = new WareData[System.Enum.GetNames(typeof(ItemType)).Length];
        foreach (ItemType cur in System.Enum.GetValues(typeof(ItemType)))
        {
            if (Categories[(int)Categorize(cur)].products.Count == 0) continue;
            foreach (var current in Categories[(int)Categorize(cur)].products)
            {
                if (current.Data.productCode == 0)
                {
                    Debug.Log(current.Data.Type + " - is added to standart");
                    Standart[(int)cur] = current;
                }
            }
        }
        
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
        Action<int> onComplete = (int designCount) =>
        {
            int preload = CountPreloaded();

            for (int i = preload; i < designCount; ++i)
            {
                WareData output = new();
                output.Data.productCode = i;
                output.Request();
                Categories[(int)Categorize(output.Data.Type)].Add(output);
            }
        };

        StartCoroutine(Php_Connect.Request_DesignCount(onComplete));
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
            case ItemType.Table:
                return ShopFilters.Paint;
            default:
                return 0;
        }
    }
    [ContextMenu("Forced set static Executor by this")]
    protected void MenuDenote()
    {
        Executor = this;
        Debug.Log("Success Denote");
    }
}
