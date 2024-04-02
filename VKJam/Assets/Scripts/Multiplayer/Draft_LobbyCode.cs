using TMPro;
using UnityEngine;
using System.Runtime.InteropServices;

public class Draft_LobbyCode : MonoBehaviour
{

    private void Awake()
    {
        GetComponent<TextMeshProUGUI>().text += LobbyManager.Instance.CurrentLobby.LobbyCode;
    }

    public void CopyToClipboard()
    {
#if UNITY_EDITOR
        GUIUtility.systemCopyBuffer = LobbyManager.Instance.CurrentLobby.LobbyCode;
#endif
        NotificationSystem.Instance.SendLocal("Скопировано");
    }
}