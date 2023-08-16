using TMPro;
using UnityEngine;

public class Draft_LobbyName : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<TextMeshProUGUI>().text = LobbyManager.Instance.LobbyName;
    }
}