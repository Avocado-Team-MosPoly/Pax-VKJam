using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetLobbyCodeAndConnect : MonoBehaviour
{
    private string connectedLobbyCode;
    [SerializeField] private Button connect;
    void Awake()
    {
        connect.onClick.AddListener(() => LobbyManager.Instance.JoinLobby(connectedLobbyCode));
    }
    public void ChangeConnectedLobbyCode(string value)
    {
        connectedLobbyCode = value;
    }

}
