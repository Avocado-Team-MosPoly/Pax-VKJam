using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class Php_Connect : MonoBehaviour
{
    public static bool PHPisOnline = true;
    
[SerializeField] private string Link;
    static public int Nickname;
    private static string link;
    private static WWW request;
    public static Currency Current;
    [System.Serializable]
    public struct Currency
    {
        public int IGCurrency;
        public int DCurrency;
    }
    void Start()
    {
        link = Link;
        PHPisOnline = true;
        /*StartCoroutine(Request_Auth(12));
        StartCoroutine(Request_DataAboutDesign(1));
        StartCoroutine(Request_CurrentCurrency("Renata"));
        StartCoroutine(Request_BuyTry("Renata",1));
        StartCoroutine(Request_CurrentCurrency("Renata"));*/
    }

    private static void ErrorProcessor(string error)
    {
        Debug.LogWarning("Server Error: " + error);
        if (error == "Cannot connect to destination host")
        {
            PHPisOnline = false;
        }
    }

    public static IEnumerator Request_Auth(int external_Nickname)
    {
        WWWForm form = new WWWForm();
        Nickname = external_Nickname;
        form.AddField("Nickname", external_Nickname);
        request = new WWW(link + "/Auth.php", form);
        yield return request;
        if (request.error != null)
        {
            ErrorProcessor(request.error);
            yield break;
        }
        Debug.Log("Server say: " + request.text);
    }
    public static IEnumerator Request_BuyTry(int DesignID)
    {
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        form.AddField("DesignID", DesignID);
        request = new WWW(link + "/BuyTry.php", form);
        yield return request;
        if (request.error != null)
        {
            ErrorProcessor(request.error);
            yield break;
        }
        Debug.Log("Server say: " + request.text);
    }
    public static IEnumerator Request_CurrentCurrency()
    {
        WWWForm form = new WWWForm();

        form.AddField("Nickname", Nickname);
        request = new WWW(link + "/CurrentCurrency.php", form);
        yield return request;
        if (request.error != null)
        {
            ErrorProcessor(request.error);
            yield break;
        }
        Debug.Log("Server say: " + request.text);
        string[] split = request.text.Split();
        Current.IGCurrency = Int32.Parse(split[0]);
        Current.DCurrency = Int32.Parse(split[1]);
    }
    public static IEnumerator Request_WhatOwnering(string Output)
    {
        WWWForm form = new WWWForm();

        form.AddField("Nickname", Nickname);
        request = new WWW(link + "/WhatOwnering.php", form);
        yield return request;
        if (request.error != null)
        {
            ErrorProcessor(request.error);
            yield break;
        }

        Debug.Log("Server say: " + request.text);
        Output = request.text;
    }
    public static IEnumerator Request_DesignCount(Action<int> onResult)
    {
        WWWForm form = new WWWForm();
        request = new WWW(link + "/DesignCount.php", form);
        yield return request;
        if (request.error != null)
        {
            ErrorProcessor(request.error);
            yield break;
        }

        Debug.Log("Server say: " + request.text);
        onResult(Int32.Parse(request.text));
    }
    public static IEnumerator Request_DataAboutDesign(int idDesign, Action<WareHouseData> onResult)
    {
        WWWForm form = new WWWForm();

        form.AddField("idDesign", idDesign);
        request = new WWW(link + "/DesignOutput.php", form);
        yield return request;
        if (request.error != null)
        {
            ErrorProcessor(request.error);
            yield break;
        }

        Debug.Log("Server say: " + request.text);
        if (request.text == "error - 404") yield break;
        string[] split = request.text.Split();
        UnityEngine.Sprite sprite = Base64ToSprite(split[1]);
        WareHouseData output = new WareHouseData(idDesign, split[0], Int32.Parse(split[2]), sprite, split[3] == "1");

        if (onResult != null)
        {
            onResult(output);
        }
    }

    private static Sprite Base64ToSprite(string base64)
    {
        byte[] bytes = System.Convert.FromBase64String(base64);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(bytes))
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        return null;
    }

}
