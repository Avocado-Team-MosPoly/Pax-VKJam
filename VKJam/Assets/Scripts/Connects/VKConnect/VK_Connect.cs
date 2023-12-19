using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class VK_Connect : TaskExecutor<VK_Connect>
{
    public UnityEvent<int> OnFriendsGot;

    public TMPro.TMP_Text DebugingText;
    public TMPro.TMP_Text NameText;
    //public URL_Image urlImage;

    [SerializeField] private bool NeedDebuging;

    [DllImport("__Internal")] private static extern void UnityPluginRequestJs();
    [DllImport("__Internal")] private static extern void UnityPluginRequestUserData();
    [DllImport("__Internal")] private static extern void UnityPluginRequestAds();
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

    public void RequestJs() // �������� �� ������� unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestJs();
#endif
    }
    public void RequestAds() // �������� �� ������� unity
    {
        if (!AdManager.CanShowAd()) return;
#if UNITY_WEBGL && !UNITY_EDITOR
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

    public void ResponseSuccessAds() // �������� �� ������� unity
    {
        AdManager.OnAdWatched();

        int tokenCount = 50;
        Action successRequest = () =>
        {
            Logger.Instance.LogError(this, "DonatRouter.Executor = " + DonatRouter.Executor);
            Logger.Instance.LogError(this, "CurrencyCatcher.Executor = " + CurrencyCatcher.Executor);
            StartCoroutine(DonatRouter.Executor?.DelayRefresh());
            CurrencyCatcher.Executor?.Refresh();
        };

        StartCoroutine(Php_Connect.Request_TokenWin(tokenCount, successRequest, null));
    }

    public void ResponseSuccessBuyDonat()
    {
        CurrencyCatcher.Executor?.Refresh();
    }

    public void ResponseGetFriends(int id)
    {
        //string[] splittedUid = uids.Split(' ');
        //int[] uidsArray = new int[splittedUid.Length - 1];

        //for (int i = 0; i < uidsArray.Length; i++)
        //{

        //}
        //if (int.TryParse(id, out int numberId))
        OnFriendsGot?.Invoke(id);
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