using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable] 
public class Currency
{
    public int IGCurrency;
    public int DCurrency;
    public int CardPiece;
}

public class Php_Connect : TaskExecutor<Php_Connect>
{
    public static bool PHPisOnline { get; private set; }
    public static int Nickname {  get; private set; }
    public static Currency Current { get; private set; } = new Currency()
    {
        IGCurrency = 100,
        DCurrency = 100,
        CardPiece = 100
    };
 
    public static RandomItemList RandomBase => Executor.randomBase;

    [SerializeField] private string Link;
    [SerializeField] private RandomItemList randomBase;

    private static string link;

    private static void ErrorProcessor(string error, string userModule)
    {
        Logger.Instance.LogError(typeof(Php_Connect), $"Server error in request to user module {userModule}: {error}");

        if (error == "Cannot connect to destination host")
        {
            PHPisOnline = false;
            Logger.Instance.LogWarning(typeof(Php_Connect), $"{nameof(Php_Connect)} is no longer online");
        }
    }

    public IEnumerator Init()
    {
        if (Link.Contains("https"))
        {
            link = Link;
        }
        else
        {
            link = string.Empty;
            PHPisOnline = false;
            Logger.Instance.LogError(this, new FormatException($"Unsafe or incorrect {nameof(Link)}. {nameof(Link)} should start with \"https\". {nameof(Link)}: {Link}"));
            yield break;
        }

        Logger.Instance.Log(this, "Initialized");
    }

    #region Requests

    private static IEnumerator Get(string userModule, Action<string> completed)
    {
        if (string.IsNullOrEmpty(link))
        {
            Logger.Instance.LogError(typeof(Php_Connect), new FormatException($"{nameof(link)} is null or empty"));
            yield break;
        }

        using UnityWebRequest request = UnityWebRequest.Get(link + $"/{userModule}");
        request.certificateHandler = new AcceptAllCertificates();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Logger.Instance.Log(typeof(Php_Connect), "Server success response: " + request.downloadHandler.text);
            completed?.Invoke(request.downloadHandler.text);
            yield break;
        }
        else if (request.result == UnityWebRequest.Result.InProgress)
            Logger.Instance.LogError(typeof(Php_Connect), "Coroutine continue run before get response from server");
        else
            ErrorProcessor(request.error, userModule);

        completed?.Invoke(null);
    }

    private static IEnumerator Post(string userModule, WWWForm wwwForm, Action<string> completed)
    {
        if (string.IsNullOrEmpty(link))
        {
            Logger.Instance.LogError(typeof(Php_Connect), new FormatException($"{nameof(link)} is null or empty"));
            yield break;
        }

        using UnityWebRequest request = UnityWebRequest.Post(link + $"/{userModule}", wwwForm);
        request.certificateHandler = new AcceptAllCertificates();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Logger.Instance.Log(typeof(Php_Connect), $"Server success response to user module {userModule}: {request.downloadHandler.text}");
            completed?.Invoke(request.downloadHandler.text);
            yield break;
        }
        else if (request.result == UnityWebRequest.Result.InProgress)
            Logger.Instance.LogError(typeof(Php_Connect), $"Coroutine continue run before get response from server. User module: {userModule}");
        else
            ErrorProcessor(request.error, userModule);

        completed?.Invoke(null);
    }

    /// <summary> successRequest - Sends true if the user has just registered, and false if not </summary>
    public static IEnumerator Request_Auth(int external_Nickname, Action<bool> successRequest, Action unsuccessRequest)
    {
        WWWForm form = new();
        form.AddField("Nickname", external_Nickname);

        Action<string> completed = (string response) =>
        {
            if (response == null)
            {
                PHPisOnline = false;
                Logger.Instance.LogError(Executor, "Unable to authenticate on server");
                unsuccessRequest?.Invoke();
            }
            else
            {
                PHPisOnline = true;
                Nickname = external_Nickname;

                if (response == "registered")
                {
                    successRequest?.Invoke(true);
                    Logger.Instance.Log(Executor, "Registered on server");
                }
                else if (response == "authenticated")
                {
                    successRequest?.Invoke(false);
                    Logger.Instance.Log(Executor, "Authenticated on server");
                }
            }
        };

        yield return Executor.StartCoroutine(Post("Auth.php", form, completed));
    }

    public static IEnumerator Request_WhichCardInPackOwnering(int idPack, Action<string> successRequest) // ���������� ��� ���, �� �����
    {
        if (!PHPisOnline)
            yield break;

        WWWForm form = new();
        form.AddField("Nickname", Nickname);
        form.AddField("PackId", idPack);

        Action<string> completed = (response) =>
        {
            if (response == null)
                return;

            successRequest?.Invoke(response);
        };

        yield return Executor.StartCoroutine(Post("WhichCardInPackOwnering.php", form, completed));
    }

    public static IEnumerator Request_TokenBuy(int id, Action<string> successRequest, Action unsuccessRequest) // ���������� ��� ���, �� �����
    {
        if (!PHPisOnline)
        {
            unsuccessRequest?.Invoke();
            yield break;
        }

        WWWForm form = new();
        form.AddField("Nickname", Nickname);
        form.AddField("id", id);

        Action<string> completed = (string response) =>
        {
            if (response == null)
            {
                unsuccessRequest?.Invoke();
                return;
            }

            successRequest?.Invoke(response);
        };

        yield return Executor.StartCoroutine(Post("TokenBuy.php", form, completed));
    }

    public static IEnumerator Request_SaveCastom(ItemType itemType, int idDesign, Action<string> successRequest)
    {
        if (!PHPisOnline)
            yield break;

        WWWForm form = new();
        form.AddField("Nickname", Nickname);
        form.AddField("ProductType", (int)itemType);
        form.AddField("idDesign", idDesign);

        Action<string> completed = (response) =>
        {
            if (response == null)
                return;

            successRequest?.Invoke(response);
        };

        yield return Executor.StartCoroutine(Post("SaveCastom.php", form, completed));
    }

    public static IEnumerator Request_LoadCastom(Action<string> successRequest)
    {
        if (!PHPisOnline)
            yield break;

        WWWForm form = new();
        form.AddField("Nickname", Nickname);

        Action<string> completed = (response) =>
        {
            if (response == null)
                return;

            successRequest?.Invoke(response);
        };

        yield return Executor.StartCoroutine(Post("LoadCastom.php", form, completed));
    }

    public static IEnumerator Request_CraftCardTry(int idCard, bool ForThePieces, Action<string> successRequest, Action unsuccessRequest)
    {
        if (!PHPisOnline)
        {
            unsuccessRequest?.Invoke();
            yield break;
        }

        WWWForm form = new();
        form.AddField("Nickname", Nickname);
        form.AddField("ForThePieces", ForThePieces ? "1" : "0");
        form.AddField("idCard", idCard.ToString());

        Action<string> completed = (string response) =>
        {
            if (response == null)
            {
                unsuccessRequest?.Invoke();
                return;
            }

            successRequest?.Invoke(response);
        };

        yield return Executor.StartCoroutine(Post("CraftCardTry.php", form, completed));
    }

    public static IEnumerator Request_UploadData(Design Upload, Action<string> successRequest, Action unsuccessRequest)
    {
        if (!PHPisOnline)
        {
            unsuccessRequest?.Invoke();
            yield break;
        }

        string JSON = JsonUtility.ToJson(Upload);
        WWWForm form = new();
        form.AddField("JSON", JSON);

        Action<string> completed = (string response) =>
        {
            if (response == null)
            {
                unsuccessRequest?.Invoke();
                return;
            }

            successRequest?.Invoke(response);
        };

        yield return Executor.StartCoroutine(Post("UploadData.php", form, completed));
    }

    public static IEnumerator Request_CheckOwningDesign(Action<string> onComplete)
    {

        if (!PHPisOnline)
        {
            onComplete?.Invoke(null);
            yield break;
        }

        WWWForm form = new();
        form.AddField("Nickname", Nickname);

        Action<string> completed = (string response) =>
        {
            if (response != null)
                onComplete?.Invoke(response);
            else
                onComplete?.Invoke(null);
        };

        yield return Executor.StartCoroutine(Post("CheckOwningDesign.php", form, completed));
    }

    public static IEnumerator Request_WhatPackOwnering(Action<string> onComplete)
    {
        if (!PHPisOnline)
        {
            onComplete?.Invoke(string.Empty);
            yield break;
        }

        WWWForm form = new();
        form.AddField("Nickname", Nickname);

        Action<string> completed = (string response) =>
        {
            onComplete?.Invoke(response ?? string.Empty);
        };

        yield return Executor.StartCoroutine(Post("WhatPackOwnering.php", form, completed));
    }

    public static IEnumerator Request_TokenWin(int Count, Action successRequest, Action unsuccessRequest)
    {
        if (!PHPisOnline)
        {
            unsuccessRequest?.Invoke();
            yield break;
        }

        WWWForm form = new();
        form.AddField("Nickname", Nickname);
        form.AddField("Count", Count);

        Action<string> completed = (string response) =>
        {
            if (response == null)
                unsuccessRequest?.Invoke();
            else
                successRequest?.Invoke();
        };

        yield return Executor.StartCoroutine(Post("TokenWin.php", form, completed));
    }

    public static IEnumerator Request_BuyTry(int DesignID, Action<string> successRequest, Action unsuccessRequest)
    {
        if (!PHPisOnline)
        {
            unsuccessRequest?.Invoke();
            yield break;
        }

        WWWForm form = new();
        form.AddField("Nickname", Nickname);
        form.AddField("DesignID", DesignID);

        Action<string> completed = (string response) =>
        {
            if (response == null)
                unsuccessRequest?.Invoke();
            else
                successRequest?.Invoke(response);
        };

        yield return Executor.StartCoroutine(Post("BuyTry.php", form, completed));
    }

    private static IEnumerator Request_CardWin(Action<string> successRequest, Action unsuccessRequest)
    {
        if (!PHPisOnline)
        {
            unsuccessRequest?.Invoke();
            yield break;
        }

        WWWForm form = new();
        form.AddField("Nickname", Nickname);

        Action<string> completed = (string response) =>
        {
            if (response == null)
                unsuccessRequest?.Invoke();
            else
                successRequest?.Invoke(response);
        };

        yield return Executor.StartCoroutine(Post("CardWin.php", form, completed));
    }

    private static IEnumerator Request_DesignWin(Action<string> successRequest, Action unsuccessRequest)
    {
        if (!PHPisOnline)
        {
            unsuccessRequest?.Invoke();
            yield break;
        }

        WWWForm form = new();
        form.AddField("Nickname", Nickname);

        Action<string> completed = (string response) =>
        {
            if (response == null)
                unsuccessRequest?.Invoke();
            else
                successRequest?.Invoke(response);
        };

        yield return Executor.StartCoroutine(Post("DesignWin.php", form, completed));
    }

    private static IEnumerator Response_Gift(int DesignID, int TargetNickname)
    {
        if (!PHPisOnline)
            yield break;

        WWWForm form = new();
        form.AddField("TargetNickname", TargetNickname);
        form.AddField("DesignID", DesignID);

        //Action<string> completed = (string response) => { };

        yield return Executor.StartCoroutine(Post("Gift.php", form, null));

    }

    public static IEnumerator Request_CurrentCurrency(Action<Currency> onComplete)
    {
        if (!PHPisOnline)
        {
            onComplete?.Invoke(Current);
            yield break;
        }

        WWWForm form = new();
        form.AddField("Nickname", Nickname);

        Action<string> completed = (string response) =>
        {
            //string[] split = response.Split();

            if (response != null)
                Current = JsonUtility.FromJson<Currency>(response);

            onComplete?.Invoke(Current);
        };

        yield return Executor.StartCoroutine(Post("CurrentCurrency.php", form, completed));
    }

    //public static string Request_WhatOwnering()
    //{
    //    if (!PHPisOnline) return "";
    //    WWWForm form = new WWWForm();

    //    form.AddField("Nickname", Nickname);

    //    using (UnityWebRequest www = UnityWebRequest.Post(link + "/WhatOwnering.php", form))
    //    {
    //        www.certificateHandler = new AcceptAllCertificates();
    //        // ������ ����������� ��������� ��� ����������
    //        www.SendWebRequest();
    //        while (!www.isDone) { }
    //        if (www.result != UnityWebRequest.Result.Success)
    //        {
    //            ErrorProcessor(www.error);
    //            return www.error;
    //        }
    //        else
    //        {
    //            Debug.Log("Server response: " + www.downloadHandler.text);
    //            return www.downloadHandler.text;
    //        }
    //    }
    //}

    public static IEnumerator Request_DesignCount(Action<int> onComplete)
    {
        if (!PHPisOnline)
        {
            onComplete?.Invoke(-1);
            yield break;
        }

        // switch to GET and check workability
        WWWForm form = new();

        Action<string> completed = (string response) =>
        {
            if (response == null)
                onComplete?.Invoke(-1);
            else
                onComplete?.Invoke(int.Parse(response));
        };

        yield return Executor.StartCoroutine(Post("DesignCount.php", form, completed));
    }

    public static IEnumerator Request_DataAboutDesign(int idDesign, Action<Design> onComplete)
    {
        if (!PHPisOnline)
        {
            onComplete?.Invoke(new Design());
            yield break;
        }

        WWWForm form = new();
        form.AddField("idDesign", idDesign);
        form.AddField("Nickname", Nickname);

        Action<string> completed = (string response) =>
        {
            if (response == null)
                onComplete?.Invoke(new Design());
            else
                onComplete?.Invoke(JsonUtility.FromJson<Design>(response));
        };

        yield return Executor.StartCoroutine(Post("DesignOutput_JSON.php", form, completed));
    }

    public static IEnumerator Request_Gift(int DesignID, int TargetNickname)
    {
        if (!PHPisOnline)
            yield break;

        bool flag = true;
        Action<string> stringSuccessRequest = (string response) =>
        {
            CurrencyCatcher.Executor.Refresh();
            flag = response != "success";
        };

        yield return Executor.StartCoroutine(Request_BuyTry(DesignID, stringSuccessRequest, null));
        if (flag)
        {
            NotificationSystem.Instance.SendLocal("Не хватает жетонов");
            yield break;
        }

        if (DesignID == 0)
        {
            int ID = RandomBase.Interact();

            if (ID >= -5 && ID < 0)
            {
                int tokenCount = RandomToken(ID);
                Action successRequest = () => Catcher_RandomItem.Executor.UIWin(RandomType.Token, tokenCount);

                Executor.StartCoroutine(Request_TokenWin(tokenCount, successRequest, null));
            }
            else
            {

                switch (ID)
                {
                    case -6:
                        stringSuccessRequest = (string response) =>
                        {
                            DesignSelect temp = JsonUtility.FromJson<DesignSelect>(response);
                            Catcher_RandomItem.Executor.GenerateWin(temp);
                        };

                        Executor.StartCoroutine(Request_DesignWin(stringSuccessRequest, null));
                        break;
                    case -7:
                        stringSuccessRequest = (string response) =>
                        {
                            if (int.TryParse(response, out int cardId))
                                Catcher_RandomItem.Executor.UIWin(RandomType.Card, cardId);
                        };

                        Executor.StartCoroutine(Request_CardWin(stringSuccessRequest, null));
                        break;
                }
            }
        }
        else
        {
            Executor.StartCoroutine(Response_Gift(DesignID, TargetNickname));
        }
    }

    #endregion

    private void OnGameEnded(string sceneName)
    {
        Current.IGCurrency += TokenManager.TokensCount;
        if (PHPisOnline)
        {
            Action successRequest = () => Catcher_RandomItem.Executor.UIWin(RandomType.Token, TokenManager.TokensCount);

            StartCoroutine(Request_TokenWin(TokenManager.TokensCount, successRequest, null));
        }
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