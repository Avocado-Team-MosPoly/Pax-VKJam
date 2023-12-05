using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GetLobbyCodeAndConnect : MonoBehaviour
{
    private string connectedLobbyCode;
    public string LobbyCode;
    [SerializeField] private Button connect;
    [SerializeField] private TMP_InputField code;
    void Awake()
    {
        connect.onClick.AddListener(async () => await LobbyManager.Instance.JoinLobby(connectedLobbyCode));
        connect.onClick.AddListener(async () => await LobbyManager.Instance.JoinLobby(LobbyCode));
        code.onValueChanged.AddListener(ChangeConnectedLobbyCode);
    }
    public void ChangeConnectedLobbyCode(string value)
    {
        connectedLobbyCode = value;
        LobbyCode = code.text;
    }
}