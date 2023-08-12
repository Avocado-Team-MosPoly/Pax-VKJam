using UnityEngine;
using System.Runtime.InteropServices;

public class VK_Connect : MonoBehaviour
{
    public TMPro.TMP_Text DebugingText;
    public TMPro.TMP_Text NameText;
    public URL_Image urlImage;


    [DllImport("__Internal")]
    private static extern void UnityPluginRequestJs();
    [DllImport("__Internal")] private static extern void UnityPluginRequestUserData();

    void Start()
    {
        DebugingText = GameObject.FindGameObjectWithTag("Debug").GetComponent<TMPro.TMP_Text>();
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
        UnityPluginRequestUserData();
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
        string Image_Url = text[1];
        string First_Name = text[2];
        if (NameText != null) NameText.text = First_Name;
        if (urlImage != null) urlImage.ChangeImage(Image_Url);
        if(Php_Connect.PHPisOnline) StartCoroutine(Php_Connect.Request_Auth(User_ID));
    }
}
