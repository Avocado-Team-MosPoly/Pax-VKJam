using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(RawImage))]
public class URL_Image : MonoBehaviour
{
    [SerializeField] private string url;
    private RawImage Target;
    void Awake()
    {
        Target = GetComponent<RawImage>();
        //StartCoroutine(DownloadImage());
    }
    public void ChangeImage(string NewUrl)
    {
        url = NewUrl;
        StartCoroutine(DownloadImage());
    }
    private IEnumerator DownloadImage()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            Target.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }
}
