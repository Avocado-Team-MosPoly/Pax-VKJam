using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class URL_Image : MonoBehaviour
{
    [SerializeField] private string url;
    private RawImage Target;

    public static Texture ProfileTexture { get; private set; }

    void Awake()
    {
        Target = GetComponent<RawImage>();
        StartCoroutine(DownloadImage());
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

        if (UnityWebRequest.Result.ConnectionError == request.result || UnityWebRequest.Result.ProtocolError == request.result)
            Debug.Log(request.error);
        else
        {
            ProfileTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Target.texture = ProfileTexture;
        }
    }
}