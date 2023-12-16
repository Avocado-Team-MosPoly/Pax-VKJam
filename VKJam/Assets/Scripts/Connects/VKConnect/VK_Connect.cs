using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class VK_Connect : TaskExecutor<VK_Connect>
{
    public TMPro.TMP_Text DebugingText;
    public TMPro.TMP_Text NameText;
    //public URL_Image urlImage;

    [SerializeField] private bool NeedDebuging;

    [DllImport("__Internal")] private static extern void UnityPluginRequestJs();
    [DllImport("__Internal")] private static extern void UnityPluginRequestUserData();
    [DllImport("__Internal")] private static extern void UnityPluginRequestAds();
    [DllImport("__Internal")] private static extern void UnityPluginRequestRepost();
    [DllImport("__Internal")] private static extern void UnityPluginRequestInviteNewPlayer();
    [DllImport("__Internal")] private static extern void UnityPluginRequestInviteOldPlayer();
    [DllImport("__Internal")] private static extern void UnityPluginRequestBuyTry(int id);
    private static VK_Connect instance;

    public IEnumerator Init()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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

    public void RequestJs() // вызываем из событий unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestJs();
#endif
    }
    public void RequestAds() // вызываем из событий unity
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
    public void RequestInvateOldPlayer() // вызываем из событий unity
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginRequestInviteOldPlayer();
#endif
    }
    public void ResponseSuccessAds() // вызываем из событий unity
    {
        AdManager.OnAdWatched();

        int tokenCount = 50;
        Action successRequest = () =>
        {
            Catcher_RandomItem._executor.UIWin(RandomType.Token, tokenCount);
            CurrencyCatcher._executor.Refresh();
        };

        StartCoroutine(Php_Connect.Request_TokenWin(tokenCount, successRequest, null));
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
    public void ResponseOk(string message)
    {
        DebugingText.text = message;
    }
    public void ResponseError(string message)
    {
        DebugingText.text = message;
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
}