using TMPro;
using UnityEngine;

public class Draft_LobbyCode : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<TextMeshProUGUI>().text += LobbyManager.Instance.CurrentLobby.LobbyCode;
    }
}