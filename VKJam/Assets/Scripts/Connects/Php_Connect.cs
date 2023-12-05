using UnityEngine.Networking;
using UnityEngine;
[System.Serializable] 
public class Currency
{
    public int IGCurrency;
    public int DCurrency;
    public int CardPiece;
}
[System.Serializable]
public class Php_Connect : MonoBehaviour
{
    public static bool PHPisOnline = true;
    
    [SerializeField] private string Link;
    [SerializeField] private RandomItemList RandomBase;
    [SerializeField] private Sprite ConnectionError;
    static public int Nickname;
    private static string link;
    public static Sprite connectionError;
    public static RandomItemList randomBase;
    public static Currency Current;
    public Currency current;
    [ContextMenu("Forced set static data by local data")]
    public void ForcedLinked()
    {
        link = Link;
        Nickname = 333;
        randomBase = RandomBase;
    }
    public void Init()
    {
        SceneLoader.EndLoad += OnGameEnded;

        if (Link.Contains("https"))
        {
            link = Link;
        }
        else
        {
            link = string.Empty;
            Logger.Instance.LogError(this, new System.FormatException($"Unsafe or incorrect {nameof(Link)}. {nameof(Link)} should start with \"https\". {nameof(Link)}: {Link}"));
            return;
        }

        randomBase = RandomBase;
        PHPisOnline = true;
        Nickname = 333;
        Request_Auth(Nickname);
        if (PHPisOnline == false) Current = current;
        //Debug.Log(Php_Connect.Request_WhichCardInPackOwnering(0));
        /*Debug.Log(Request_BuyTry(0));
        StartCoroutine(Request_Auth(12));
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
    
    public static string Request_WhichCardInPackOwnering(int idPack) // Переписать под инт, по схеме
    {
        if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        form.AddField("PackId", idPack);

        using (UnityWebRequest www = UnityWebRequest.Post(link + "/WhichCardInPackOwnering.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }
    public static string Request_TokenBuy(int id) // Переписать под инт, по схеме
    {
        if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        form.AddField("Id", id);

        using (UnityWebRequest www = UnityWebRequest.Post(link + "/TokenBuy.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }
    public static string Request_CraftCardTry(int idCard, bool ForThePieces)
    {
        if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        form.AddField("ForThePieces", ForThePieces.ToString());
        form.AddField("idCard", idCard.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(link + "/CraftCardTry.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }
    public static string Request_WhatPackOwnering() 
    {
        if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/WhatPackOwnering.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }
    private void OnGameEnded(string sceneName)
    {

        Current.IGCurrency += TokenManager.TokensCount;
        if (PHPisOnline) Request_TokenWin(TokenManager.TokensCount);
    }
    public static string Request_TokenWin(int Count)
    {
        if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        form.AddField("Count", Count);
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/TokenWin.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }
    public static string Request_BuyTry(int DesignID)
    {
        if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        form.AddField("DesignID", DesignID);
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/BuyTry.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }
    public static string Request_Gift(int DesignID, int TargetNickname)
    {
        if (!PHPisOnline) return "";
        string result = Request_BuyTry(DesignID);
        if (result != "success") return result;
        int ID;
        if (DesignID == 0)
        {
            ID = randomBase.Interact();
            if (ID >= -5 && ID < 0) return Request_TokenWin(RandomToken(ID));
            else switch (ID)
                {
                    case -6:
                        return Request_DesignWin();
                    case -7:
                        return Request_CardWin(false);
                    case -8:
                        return Request_CardWin(true);
                    default:
                        return "";
                }
        }
        else return Response_Gift(DesignID, TargetNickname);
    }
    private static int RandomToken(int id)
    {
        switch (id)
        {
            case -1:
                return Random.Range(1, 50);
            case -2:
                return Random.Range(51, 100);
            case -3:
                return Random.Range(101, 200);
            case -4:
                return Random.Range(201, 250);
            case -5:
                return Random.Range(251, 500);
            default:
                return 0;
        }
    }
    private static string Response_Gift(int DesignID, int TargetNickname)
    {
        if (!PHPisOnline) return "";
        int ID = DesignID;
        WWWForm form = new WWWForm();
        form.AddField("TargetNickname", TargetNickname);
        form.AddField("DesignID", ID);
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/Gift.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }

    }
    public static string Request_Auth(int external_Nickname)
    {
        WWWForm form = new WWWForm();
        Nickname = external_Nickname;
        form.AddField("Nickname", external_Nickname);
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/Auth.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }
    public static Currency Request_CurrentCurrency()
    {
        if (!PHPisOnline) return Current;
        WWWForm form = new WWWForm();

        form.AddField("Nickname", Nickname);
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/CurrentCurrency.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return Current;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                string[] split = www.downloadHandler.text.Split();
                Current = JsonUtility.FromJson<Currency>(www.downloadHandler.text);
                return Current;
            }
        }
    }

    public static string Request_WhatOwnering()
    {
        if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();

        form.AddField("Nickname", Nickname);
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/WhatOwnering.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }
    public static string Request_CardWin(bool isShard)
    {
        //if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        Debug.Log(link + "/CardWin.php");
        form.AddField("Nickname", Nickname);
        form.AddField("IsShard", isShard.ToString());
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/CardWin.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }
    public static string Request_DesignWin()
    {
        //if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        Debug.Log(link + "/DesignWin.php");
        form.AddField("Nickname", Nickname);
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/DesignWin.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }
    public static int Request_DesignCount()
    {
        if (!PHPisOnline) return -1;
        WWWForm form = new WWWForm();

        using (UnityWebRequest www = UnityWebRequest.Post(link + "/DesignCount.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return -1;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return int.Parse(www.downloadHandler.text);
            }
        }
    }
    public static Design Request_DataAboutDesign(int idDesign)
    {
        if (!PHPisOnline) return new Design();
        WWWForm form = new WWWForm();
        form.AddField("idDesign", idDesign);
        form.AddField("Nickname", Nickname);
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/DesignOutput_JSON.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // Запрос выполняется дожидаясь его завершения
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return new Design();
            }
            else if (www.downloadHandler.text == "error - 404")
            {
                ErrorProcessor("404");
                return new Design();
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return JsonUtility.FromJson<Design>(www.downloadHandler.text);
            }
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
