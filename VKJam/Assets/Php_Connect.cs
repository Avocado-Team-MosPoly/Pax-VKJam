using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class Php_Connect : MonoBehaviour
{
    [SerializeField] public string Link;
    private static string link;
    private static WWW request;
    public static Currency Current;
    [System.Serializable]
    public struct Currency
    {
        public int IGCurrency;
        public int DCurrency;
    }
    void Start()
    {
        link = Link;
        /*StartCoroutine(Request_CurrentCurrency("Renata"));
        StartCoroutine(Request_BuyTry("Renata",1));
        StartCoroutine(Request_CurrentCurrency("Renata"));*/
    }
    static IEnumerator Request_BuyTry(string Nickname, int DesignID)
    {
        WWWForm form = new WWWForm();
        form.AddField("Nickname", Nickname);
        form.AddField("DesignID", DesignID);
        request = new WWW(link + "/BuyTry.php", form);
        yield return request;
        if(request.error != null) 
        { 
            Debug.LogWarning("Server Error: " + request.error); 
            yield break; 
        }
        Debug.Log("Server say: " + request.text);
    }
    static IEnumerator Request_CurrentCurrency(string Nickname)
    {
        WWWForm form = new WWWForm();
        
        form.AddField("Nickname", Nickname);
        request = new WWW(link + "/CurrentCurrency.php", form);
        yield return request;
        if (request.error != null)
        {
            Debug.LogWarning("Server Error: " + request.error);
            yield break;
        }
        Debug.Log("Server say: " + request.text);
        string[] split = request.text.Split();
        Current.IGCurrency = Int32.Parse(split[0]);
        Current.DCurrency = Int32.Parse(split[1]);
    }
}
