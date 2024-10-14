using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;

public class VK_Connect : BaseSingleton<VK_Connect>
{
    public Action<bool> OnInterstitialAdTryWatched;
    public Action<int[]> OnFriendsGot;

    public VariableObserver<bool> IsJoinedVKGroupObserver { get; private set; } = new(false);

    [Header("Debug")]

    [SerializeField] private bool NeedDebuging;
    [SerializeField] private TMP_Text DebugingText;
    [SerializeField] private TMP_Text NameText;

    public bool IsLoggedInThroughVK;

    #region External Methods

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
    [DllImport("__Internal")] private static extern void UnityPluginRequestCheckSubscriptionVKGroup();

    #endregion

    public IEnumerator Init()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (Instance == this)
        {
            DontDestroyOnLoad(Instance);
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

    public void RequestJs() // �������� �� ������� unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestJs();
#endif
    }

    public IEnumerator RequestShowInterstitialAd() // �������� �� ������� unity
    {
        yield return new WaitForSeconds(3f);
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequest_ShowInterstitialAd();
#endif
    }
    public void RequestShowRewardAd() // �������� �� ������� unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!AdManager.CanShowAd())
            return;

        UnityPluginRequestAds();
#endif
    }

    public void RequestRepost() // �������� �� ������� unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestRepost();
#endif
    }
    public void RequestInvateNewPlayer() // �������� �� ������� unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestInviteNewPlayer();
#endif
    }
    public void RequestInvateOldPlayer(int id, string lobby_key) // �������� �� ������� unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestInviteOldPlayer(id, lobby_key);
#endif
    }

    public void RequestUserData() // �������� �� ������� unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestUserData();
#endif
    }
    public void RequestBuyTry(int id) // �������� �� ������� unity
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

    public void RequestCheckSubscriptionVKGroup()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestCheckSubscriptionVKGroup();
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
        StartCoroutine(Php_Connect.Request_AdWatched(() =>
        {
            if (DonatRouter.Instance != null)
                StartCoroutine(DonatRouter.Instance.DelayRefresh());
            if (CurrencyCatcher.Instance != null)
                CurrencyCatcher.Instance.Refresh();

            AdManager.OnAdWatched();
        }, null));

        //int tokenCount = 30;
        //Action successRequest = () =>
        //{
        //    if (DonatRouter.Instance != null)
        //        StartCoroutine(DonatRouter.Instance.DelayRefresh());
        //    if (CurrencyCatcher.Instance != null)
        //        CurrencyCatcher.Instance.Refresh();
        //};

        //StartCoroutine(Php_Connect.Request_TokenWin(tokenCount, successRequest, null));
    }

    public void Response_ShowInterstitialAd(int watchedAdsCount)
    {
        bool isWatched = watchedAdsCount > 0;
        Logger.Instance.Log(this, "Interstitial ad " + (isWatched ? "watched" : "not watched"));

        OnInterstitialAdTryWatched?.Invoke(isWatched);
    }

    public void ResponseSuccessBuyDonat()
    {
        if (CurrencyCatcher.Instance != null)
            CurrencyCatcher.Instance.Refresh();
    }

    public void ResponseGetFriends(string Input)
    {
        string[] splittedUids = Input.Split(' ');
        int[] uidsArray = new int[splittedUids.Length - 1];

        for (int i = 0; i < uidsArray.Length; i++)
            uidsArray[i] = int.Parse(splittedUids[i]);

        OnFriendsGot?.Invoke(uidsArray);
    }

    /// <summary> isSubscribed: 0 - false, else true </summary>
    public void ResponseJoinedVKGroup(int isSubscribed)
    {
        IsJoinedVKGroupObserver.Value = isSubscribed != 0;
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

    static public string EncodeTo64(string toEncode)
    {
        byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);

        string returnValue = Convert.ToBase64String(toEncodeAsBytes);

        return returnValue;
    }
}