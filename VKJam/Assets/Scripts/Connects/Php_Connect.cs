using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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
    private static string userModulesLink;
    private static string resourcesLink;
    private static void ErrorProcessor(string error)
    {
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
            userModulesLink = Link + "User/";
            resourcesLink = Link + "Resources/";
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

    private static IEnumerator GetToUserModule(string userModule, Action<string> completed)
    {
        if (string.IsNullOrEmpty(userModulesLink))
        {
            Logger.Instance.LogError(typeof(Php_Connect), new FormatException($"{nameof(userModulesLink)} is null or empty"));
            yield break;
        }

        using UnityWebRequest request = UnityWebRequest.Get(userModulesLink + userModule);
        request.certificateHandler = new AcceptAllCertificates();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Logger.Instance.Log(typeof(Php_Connect), "Server success response: " + request.downloadHandler.text);
            completed?.Invoke(request.downloadHandler.text);
            yield break;
        }
        else
        {
            Logger.Instance.LogError(typeof(Php_Connect), $"Server error in request to user module {userModule}: {request.error}");
            ErrorProcessor(request.error);
        }
        completed?.Invoke(null);
    }

    private static IEnumerator PostToUserModule(string userModule, WWWForm wwwForm, Action<string> completed)
    {
        if (string.IsNullOrEmpty(userModulesLink))
        {
            Logger.Instance.LogError(typeof(Php_Connect), new FormatException($"{nameof(userModulesLink)} is null or empty"));
            yield break;
        }

        using UnityWebRequest request = UnityWebRequest.Post(userModulesLink + userModule, wwwForm);
        request.certificateHandler = new AcceptAllCertificates();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Logger.Instance.Log(typeof(Php_Connect), $"Server success response to user module {userModule}: {request.downloadHandler.text}");
            completed?.Invoke(request.downloadHandler.text);
            yield break;
        }
        else
        {
            Logger.Instance.LogError(typeof(Php_Connect), $"Server error in request to user module {userModulesLink + userModule}: {request.error}");
            ErrorProcessor(request.error);
        }

        completed?.Invoke(null);
    }

    private static IEnumerator GetTexture(string pathFromResources, Action<Texture2D> completed)
    {
        if (string.IsNullOrEmpty(resourcesLink))
        {
            Logger.Instance.LogError(typeof(Php_Connect), new FormatException($"{nameof(resourcesLink)} is null or empty"));
            completed?.Invoke(null);
            yield break;
        }

        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(resourcesLink + pathFromResources);

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Logger.Instance.Log(typeof(Php_Connect), "Success downloded image: Resources/" + pathFromResources);
            completed?.Invoke(DownloadHandlerTexture.GetContent(request));
            yield break;
        }
        else
        {
            Logger.Instance.LogError(typeof(Php_Connect), $"Server error in request to resource link {resourcesLink + pathFromResources}");
            ErrorProcessor(request.error);
        }
        completed?.Invoke(null);
    }

    /// <summary> successRequest - Sends true if the user has just registered, and false if not </summary>
    public static IEnumerator Request_Auth(int external_Nickname, Action<bool> successRequest, Action<string> unsuccessRequest)
    {
        WWWForm form = new();
        form.AddField("Nickname", external_Nickname);

        Action<string> completed = (string response) =>
        {
            if (response == null)
            {
                PHPisOnline = false;
                Logger.Instance.LogError(Executor, "Unable to authenticate on server");
                unsuccessRequest?.Invoke("Unable to authenticate on server exception");
            }
            else if (response == "can not log into one account from several devices")
            {
                PHPisOnline = false;
                Logger.Instance.LogError(Executor, "Unable to authenticate on server");
                unsuccessRequest?.Invoke("LogIn through several devices exception");
            }
            else if (response == "registered")
            {
                PHPisOnline = true;

                Logger.Instance.Log(Executor, "Registered on server");
                successRequest?.Invoke(true);
            }
            else if (response == "authenticated")
            {
                PHPisOnline = true;

                Logger.Instance.Log(Executor, "Authenticated on server");
                successRequest?.Invoke(false);
            }
            else
            {
                PHPisOnline = false;

                Logger.Instance.LogError(Executor, "Unable to authenticate on server");
                unsuccessRequest?.Invoke("Unexpected exception");
            }

            if (PHPisOnline)
            {
                Nickname = external_Nickname;
            }
        };

        yield return Executor.StartCoroutine(PostToUserModule("Auth.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("WhichCardInPackOwnering.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("TokenBuy.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("SaveCastom.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("LoadCastom.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("CraftCardTry.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("UploadData.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("CheckOwningDesign.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("WhatPackOwnering.php", form, completed));
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
            if (response != "success")
                unsuccessRequest?.Invoke();
            else
                successRequest?.Invoke();
        };

        yield return Executor.StartCoroutine(PostToUserModule("TokenWin.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("BuyTry.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("CardWin.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("DesignWin.php", form, completed));
    }

    private static IEnumerator Response_Gift(int DesignID, int TargetNickname)
    {
        if (!PHPisOnline)
            yield break;

        WWWForm form = new();
        form.AddField("TargetNickname", TargetNickname);
        form.AddField("DesignID", DesignID);

        //Action<string> completed = (string response) => { };

        yield return Executor.StartCoroutine(PostToUserModule("Gift.php", form, null));

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

        yield return Executor.StartCoroutine(PostToUserModule("CurrentCurrency.php", form, completed));
    }

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

        yield return Executor.StartCoroutine(PostToUserModule("DesignCount.php", form, completed));
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

        yield return Executor.StartCoroutine(PostToUserModule("DesignOutput_JSON.php", form, completed));
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

    public static IEnumerator Request_Exit()
    {
        if (!PHPisOnline)
            yield break;

        WWWForm form = new();
        form.AddField("Nickname", Nickname);

        Action<string> completed = (string response) =>
        {
            if (response == "completed")
            {
                Debug.Log("Exit from DB completed");
            }
        };

        yield return Executor.StartCoroutine(PostToUserModule("Exit.php", form, completed));
    }

    public static IEnumerator Request_MonsterTextures(string[] monsterTextureNames, Action<List<Texture2D>> onComplete)
    {
        if (!PHPisOnline)
            yield break;

        if (monsterTextureNames == null)
        {
            Logger.Instance.LogError(Executor, new NullReferenceException($"{nameof(monsterTextureNames)} is null"));
            yield break;
        }
        else if (monsterTextureNames.Length != 3)
        {
            Logger.Instance.LogError(Executor, new FormatException($"{nameof(monsterTextureNames)} has incorrect Length ({monsterTextureNames.Length}). Should be 3"));
            yield break;
        }

        List<Texture2D> textures = new(3);
        Action<Texture2D> completed = (Texture2D response) =>
        {
            if (response == null)
                Logger.Instance.LogWarning(Executor, "Texture is null");

            textures.Add(response);
        };

        /* Monster texture paths on server
         * 0 - Monster Card Texture (Resources/Monsters/CardTexture/)
         * 1 - Monster Texture (Resources/Monsters/MonsterInBestiaryTexture/)
         * 2 - Monster In Bestiary Texture (Resources/Monsters/MonsterTexture/)
         */
        yield return Executor.StartCoroutine(GetTexture($"Monsters/CardTexture/{monsterTextureNames[0]}", completed));
        yield return Executor.StartCoroutine(GetTexture($"Monsters/MonsterInBestiaryTexture/{monsterTextureNames[1]}", completed));
        yield return Executor.StartCoroutine(GetTexture($"Monsters/MonsterTexture/{monsterTextureNames[2]}", completed));

        onComplete?.Invoke(textures);
    }

    #endregion

    public void Exit()
    {
        StartCoroutine(Request_Exit());
    }

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

    private void OnApplicationQuit()
    {
        Exit();
    }
}