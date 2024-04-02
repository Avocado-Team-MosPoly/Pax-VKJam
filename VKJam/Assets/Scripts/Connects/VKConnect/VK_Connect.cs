using System;
using System.Collections;
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

    }

    public IEnumerator RequestShowInterstitialAd() // �������� �� ������� unity
    {
        yield return new WaitForSeconds(0f); // 3f
    }

    public void RequestShowRewardAd() // �������� �� ������� unity
    {

    }

    public void RequestRepost() // �������� �� ������� unity
    {

    }
    public void RequestInvateNewPlayer() // �������� �� ������� unity
    {

    }
    public void RequestInvateOldPlayer(int id, string lobby_key) // �������� �� ������� unity
    {

    }

    public void RequestUserData() // �������� �� ������� unity
    {

    }
    public void RequestBuyTry(int id) // �������� �� ������� unity
    {

    }

    public void RequestGetFriends()
    {

    }

    public void RequestJoinGroup()
    {

    }

    public void RequestCheckSubscriptionVKGroup()
    {

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