using System;
using UnityEngine;
[Serializable]
public struct Design {
    public int productCode;
    public string productName;
    [HideInInspector]public string icon;
    public int productPrice;
    public bool IsDonateVault;
    public bool InOwn;
    public ItemType Type;
}
[Serializable]
public struct DesignSelect
{
    public string text;
    public int design;
    public int type;
}

[Serializable]
public class WareData
{
    
    public Design Data;
    public bool IsNonBuyable;
    public UnityEngine.Sprite icon;
    public bool isPreloaded;
    public GameObject Model;
    
    public void Request()
    {
        if (isPreloaded)
            return;

        Data = Php_Connect.Request_DataAboutDesign(Data.productCode);
        icon = Base64ToSprite(Data.icon);
    }

    public void OnCheckOwningDesignComplete(bool result)
    {
        Data.InOwn = result;
    }

    protected static Sprite Base64ToSprite(string base64)
    {
        try
        {
            byte[] bytes = System.Convert.FromBase64String(base64);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(bytes))
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            return null;
        }
        catch (System.FormatException e)
        {
            Debug.LogError("Base64 string is not valid: " + e.Message);
            return null;
        }
    }
    [ContextMenu("Set in Choosen Custom")]
    public void ChooseThis()
    {
        //Debug.Log(CustomController._executor);
        //Debug.Log(Data.Type);
        //Debug.Log(this);
        CustomController._executor.Custom[(int)Data.Type] = this;
    }
}