using TMPro;
using UnityEngine;

public class Draft_LobbyCode : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<TextMeshProUGUI>().text += LobbyManager.Instance.CurrentLobby.LobbyCode;
    }

    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = LobbyManager.Instance.CurrentLobby.LobbyCode;
        NotificationSystem.Instance.SendLocal("Скопировано");
    }
}