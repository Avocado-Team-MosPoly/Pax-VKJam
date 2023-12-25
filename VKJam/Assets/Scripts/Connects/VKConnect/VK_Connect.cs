using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class VK_Connect : TaskExecutor<VK_Connect>
{
    public Action<bool> OnInterstitialAdTryWatched;
    public Action<int[]> OnFriendsGot;

    public TMPro.TMP_Text DebugingText;
    public TMPro.TMP_Text NameText;
    //public URL_Image urlImage;

    [SerializeField] private bool NeedDebuging;

    [DllImport("__Internal")] private static extern void UnityPluginRequestJs();
    [DllImport("__Internal")] private static extern void UnityPluginRequestUserData();
    [DllImport("__Internal")] private static extern void UnityPluginRequestAds();
    [DllImport("__Internal")] private static extern void UnityPluginRequest_ShowInterstitialAd();
    [DllImport("__Internal")] private static extern void UnityPluginRequestRepost();
    [DllImport("__Internal")] private static extern void UnityPluginRequestInviteNewPlayer();
    [DllImport("__Internal")] private static extern void UnityPluginRequestInviteOldPlayer(int id, string lobby_key);
    [DllImport("__Internal")] private static extern void UnityPluginRequestBuyTry(int id);
    [DllImport("__Internal")] private static extern void UnityPluginRequestGetFriends();
    [DllImport("__Internal")] private static extern void UnityPluginRequestJoinGroup();

    public IEnumerator Init()
    {
        if (Executor == null)
        {
            Executor = this;
        }
        
        if (Executor == this)
        {
            DontDestroyOnLoad(Executor);
        }
        else
        {
            Logger.Instance.LogError(this, $"Two or more {nameof(VK_Connect)} on scene");
            Destroy(this);
            yield break;
        }

        yield return new WaitForSeconds(1);
        GameObject temp;

        if (NeedDebuging)
        {
            if (DebugingText == null)
            {
                temp = GameObject.FindGameObjectWithTag("Debug");
                if (temp != null)
                    DebugingText = temp.GetComponent<TMPro.TMP_Text>();
            }
            if (NameText == null)
            {
                temp = GameObject.FindGameObjectWithTag("Name");
                if (temp != null)
                    NameText = temp.GetComponent<TMPro.TMP_Text>();
            }
            //if (urlImage == null)
            //{
            //    temp = GameObject.FindGameObjectWithTag("Profile_Picture");
            //    if (temp != null)
            //        urlImage = temp.GetComponent<URL_Image>();
            //}
        }

        RequestUserData();

        Logger.Instance.Log(this, "Initialized");
    }

    #region Requests

    public void RequestJs() // вызываем из событий unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestJs();
#endif
    }

    public IEnumerator RequestShowInterstitialAd() // вызываем из событий unity
    {
        yield return new WaitForSeconds(1);
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequest_ShowInterstitialAd();
#endif
    }

    public void RequestShowRewardAd() // вызываем из событий unity
    {
        if (!AdManager.CanShowAd()) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestAds();
#endif
    }
    public void RequestRepost() // вызываем из событий unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestRepost();
#endif
    }
    public void RequestInvateNewPlayer() // вызываем из событий unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestInviteNewPlayer();
#endif
    }
    public void RequestInvateOldPlayer(int id, string lobby_key) // вызываем из событий unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestInviteOldPlayer(id, lobby_key);
#endif
    }

    public void RequestUserData() // вызываем из событий unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestUserData();
#endif
    }
    public void RequestBuyTry(int id) // вызываем из событий unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestBuyTry(id);
#endif
    }

    public void RequestGetFriends()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestGetFriends();
#endif
    }

    public void RequestJoinGroup()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestJoinGroup();
#endif
    }

    #endregion

    #region Responses

    public void ResponseOk(string message)
    {
        DebugingText.text = message;
    }
    public void ResponseError(string message)
    {
        DebugingText.text = message;
    }

    public void ResponseSuccessAds()
    {
        AdManager.OnAdWatched();

        int tokenCount = 30;
        Action successRequest = () =>
        {
            StartCoroutine(DonatRouter.Executor?.DelayRefresh());
            CurrencyCatcher.Executor?.Refresh();
        };

        StartCoroutine(Php_Connect.Request_TokenWin(tokenCount, successRequest, null));
    }

    public void Response_ShowInterstitialAd(bool isWatched)
    {
        Logger.Instance.Log(this, "Interstitial ad " + (isWatched ? "watched" : "not watched"));

        OnInterstitialAdTryWatched?.Invoke(isWatched);
    }

    public void ResponseSuccessBuyDonat()
    {
        CurrencyCatcher.Executor?.Refresh();
    }

    public void ResponseGetFriends(string Input)
    {
        string[] splittedUids = Input.Split(' ');
        int[] uidsArray = new int[splittedUids.Length - 1];

        Logger.Instance.LogError(this, splittedUids[^1]);

        for (int i = 0; i < uidsArray.Length; i++)
            uidsArray[i] = int.Parse(splittedUids[i]);

        OnFriendsGot?.Invoke(uidsArray);
    }

    public void UserData_Processing(string Input)
    {
        string[] text = Input.Split('|');
        UserData.UserId = int.Parse(text[0]);
        //string userIMG_URL = text[1];
        UserData.UserName = text[2];

        if (NameText != null)
            NameText.text = UserData.UserName;
        //if (urlImage != null)
        //  urlImage.ChangeImage(UserData.UserIMG_URL);
    }

#endregion
}