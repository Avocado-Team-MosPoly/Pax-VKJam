using TMPro;
using UnityEngine;
using System.Runtime.InteropServices;

public class Draft_LobbyCode : MonoBehaviour
{
    [DllImport("__Internal")] private static extern void UnityPluginCopyTextToClipboard(string text);

    private void Awake()
    {
        GetComponent<TextMeshProUGUI>().text += LobbyManager.Instance.CurrentLobby.LobbyCode;
    }

    public void CopyToClipboard()
    {
#if UNITY_EDITOR
        GUIUtility.systemCopyBuffer = LobbyManager.Instance.CurrentLobby.LobbyCode;
#elif UNITY_WEBGL && !UNITY_EDITOR
        UnityPluginCopyTextToClipboard(LobbyManager.Instance.CurrentLobby.LobbyCode);
#endif
        NotificationSystem.Instance.SendLocal("Скопировано");
    }
}