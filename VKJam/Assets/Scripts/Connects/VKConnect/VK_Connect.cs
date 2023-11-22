using UnityEngine;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public class VK_Connect : MonoBehaviour
{
    public TMPro.TMP_Text DebugingText;
    public TMPro.TMP_Text NameText;
    public URL_Image urlImage;

    [SerializeField] private Php_Connect PHPConnect;


    [DllImport("__Internal")]
    private static extern void UnityPluginRequestJs();
    [DllImport("__Internal")] private static extern void UnityPluginRequestUserData();

    private static VK_Connect instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        Init();
        PHPConnect.Init();
    }

    private void Init()
    {
        GameObject temp;
        if (DebugingText == null)
        {
            temp = GameObject.FindGameObjectWithTag("Debug");
            if (temp != null) DebugingText = temp.GetComponent<TMPro.TMP_Text>();
        }
        if (NameText == null) { 
            temp = GameObject.FindGameObjectWithTag("Name");
            if(temp != null) NameText = temp.GetComponent<TMPro.TMP_Text>();
        }
        if (urlImage == null) { 
            temp = GameObject.FindGameObjectWithTag("Profile_Picture");
            if (temp != null) urlImage = temp.GetComponent<URL_Image>();
        }
        RequestUserData();
    }

    public void ButClick()
    {
        RequestJs();
    }
    public void RequestJs() // вызываем из событий unity
    {
        UnityPluginRequestJs();
    }
    public void RequestUserData() // вызываем из событий unity
    {
#if UNITY_EDITOR
#else
        UnityPluginRequestUserData();
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
        int User_ID = int.Parse(text[0]);
        UserData.UserIMG_URL = text[1];
        UserData.UserName = text[2];
        if (NameText != null) NameText.text = UserData.UserName;
        if (urlImage != null) urlImage.ChangeImage(UserData.UserIMG_URL);
        if(Php_Connect.PHPisOnline) StartCoroutine(Php_Connect.Request_Auth(User_ID));

        Authentication.LogInVK(User_ID.ToString(), UserData.UserName);
    }
}