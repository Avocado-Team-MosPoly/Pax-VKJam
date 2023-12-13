using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

[System.Serializable] 
public class Currency
{
    public int IGCurrency;
    public int DCurrency;
    public int CardPiece;
}

[System.Serializable]
public class Php_Connect : TaskExecutor<Php_Connect>
{
    public RandomItemList RandomBase;
    public Currency current;

    public static bool PHPisOnline = true;

    public static int Nickname;
    public static Sprite connectionError;
    public static RandomItemList randomBase;
    public static Currency Current;

    [SerializeField] private string Link;
    [SerializeField] private Sprite ConnectionError;

    private static string link;

    [ContextMenu("Forced set static data by local data")]
    public void ForcedLinked()
    {
        link = Link;
        Nickname = 333;
        randomBase = RandomBase;
    }

    public IEnumerator Init()
    {
        Logger.Instance.Log(this, "Initialization started");

        //SceneLoader.EndLoad += OnGameEnded;

        if (Link.Contains("https"))
        {
            link = Link;
        }
        else
        {
            link = string.Empty;
            PHPisOnline = false;
            Logger.Instance.LogError(this, new System.FormatException($"Unsafe or incorrect {nameof(Link)}. {nameof(Link)} should start with \"https\". {nameof(Link)}: {Link}"));
            yield break;
        }

        randomBase = RandomBase;
        PHPisOnline = true;
        Nickname = 333;

        if (PHPisOnline == false)
            Current = current;

        Logger.Instance.Log(this, "Initialization ended");

        //Debug.Log(Php_Connect.Request_WhichCardInPackOwnering(0));
        //Debug.Log(Request_BuyTry(0));
        //StartCoroutine(Request_Auth(12));
        //StartCoroutine(Request_DataAboutDesign(1));
        //StartCoroutine(Request_CurrentCurrency("Renata"));
        //StartCoroutine(Request_BuyTry("Renata",1));
        //StartCoroutine(Request_CurrentCurrency("Renata"));
    }

    private static void ErrorProcessor(string error)
    {
        Debug.LogError("Server Error: " + error);
        if (error == "Cannot connect to destination host")
        {
            PHPisOnline = false;
        }
    }

    public static string Request_WhichCardInPackOwnering(int idPack) // ѕереписать под инт, по схеме
    {
        if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        form.AddField("PackId", idPack);

        using (UnityWebRequest www = UnityWebRequest.Post(link + "/WhichCardInPackOwnering.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // «апрос выполн€етс€ дожида€сь его завершени€
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
    public static string Request_TokenBuy(int id) // ѕереписать под инт, по схеме
    {
        if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        form.AddField("Id", id);

        using (UnityWebRequest www = UnityWebRequest.Post(link + "/TokenBuy.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // «апрос выполн€етс€ дожида€сь его завершени€
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
            // «апрос выполн€етс€ дожида€сь его завершени€
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
    public static string Request_UploadData(WareData Upload)
    {
        return Request_UploadData(Upload.Data);
    }
    public static string Request_UploadData(Design Upload)
    {
        string JSON = JsonUtility.ToJson(Upload);
        if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        form.AddField("JSON", JSON);

        using (UnityWebRequest www = UnityWebRequest.Post(link + "/UploadData.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // «апрос выполн€етс€ дожида€сь его завершени€
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
    public static IEnumerator Request_CheckOwningDesign(int idDesign, System.Action<bool> onComplete)
    {
        if (idDesign == 0)
        {
            onComplete?.Invoke(true);
            yield break;
        }
        if (!PHPisOnline)
        {
            onComplete?.Invoke(false);
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("idDesign", idDesign);
        form.AddField("Nickname", Nickname);

        using (UnityWebRequest www = UnityWebRequest.Post(link + "/CheckOwningDesign.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Can not load element with idDesign {idDesign}");
                ErrorProcessor(www.error);
                onComplete?.Invoke(false);
            }
            else if (www.downloadHandler.text == "error - 404")
            {
                ErrorProcessor("404");
                onComplete?.Invoke(false);
            }
            else
            {
                //Debug.Log("Server response: " + www.downloadHandler.text);
                //Debug.Log($"Loaded element with idDesign { idDesign}");
                onComplete?.Invoke(www.downloadHandler.text == "true");
            }
        }
    }
    public static bool Request_CheckOwningDesign(int idDesign)
    {
        if (idDesign == 0) return true;
        if (!PHPisOnline) return false;
        WWWForm form = new WWWForm();
        form.AddField("idDesign", idDesign);
        form.AddField("Nickname", Nickname);
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/CheckOwningDesign.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // «апрос выполн€етс€ дожида€сь его завершени€
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return false;
            }
            else if (www.downloadHandler.text == "error - 404")
            {
                ErrorProcessor("404");
                return false;
            }
            else
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                return www.downloadHandler.text == "true" ? true : false;
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
            // «апрос выполн€етс€ дожида€сь его завершени€
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
            // «апрос выполн€етс€ дожида€сь его завершени€
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorProcessor(www.error);
                return www.error;
            }
            else
            {
                Catcher_RandomItem._executor.UIWin(RandomType.Token,Count);
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
            // «апрос выполн€етс€ дожида€сь его завершени€
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
            Debug.Log(ID);
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
                return UnityEngine.Random.Range(1, 50);
            case -2:
                return UnityEngine.Random.Range(51, 100);
            case -3:
                return UnityEngine.Random.Range(101, 200);
            case -4:
                return UnityEngine.Random.Range(201, 250);
            case -5:
                return UnityEngine.Random.Range(251, 500);
            default:
                return 0;
        }
    }
    private static string Request_CardWin(bool isShard)
    {
        if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        form.AddField("IsShard", isShard.ToString());
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/CardWin.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // «апрос выполн€етс€ дожида€сь его завершени€
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
                Catcher_RandomItem._executor.UIWin(RandomType.Card, int.Parse(www.downloadHandler.text));
                return www.downloadHandler.text;
            }
        }
    }
    private static string Request_DesignWin()
    {
        //if (!PHPisOnline) return "";
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/DesignWin.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // «апрос выполн€етс€ дожида€сь его завершени€
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

                DesignSelect temp = JsonUtility.FromJson<DesignSelect>(www.downloadHandler.text);
                Catcher_RandomItem._executor.GenerateWin(temp);
                return www.downloadHandler.text;
            }
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
            // «апрос выполн€етс€ дожида€сь его завершени€
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
    public static IEnumerator Request_Auth(int external_Nickname)
    {
        WWWForm form = new WWWForm();
        Nickname = external_Nickname;
        form.AddField("Nickname", external_Nickname);
        
        using (UnityWebRequest www = UnityWebRequest.Post(link + "/Auth.php", form))
        {
            www.certificateHandler = new AcceptAllCertificates();
            // «апрос выполн€етс€ дожида€сь его завершени€
            float startTime = Time.time;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success  && www.isDone)
            {
                //ErrorProcessor(www.error);
                Logger.Instance.LogError(_executor, "Unable to authenticate on server: " + www.error);
                //return www.error;
            }
            else
            {
                Logger.Instance.Log(_executor, "Authenticated on server: " + www.downloadHandler.text);
                //return www.downloadHandler.text;
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
            // «апрос выполн€етс€ дожида€сь его завершени€
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
            // «апрос выполн€етс€ дожида€сь его завершени€
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
            // «апрос выполн€етс€ дожида€сь его завершени€
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
            // «апрос выполн€етс€ дожида€сь его завершени€
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
