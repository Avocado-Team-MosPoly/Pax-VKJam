using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuickLobbyButton : MonoBehaviour
{
    private Button _button;

    private void OnEnable()
    {
        if(_button == null)
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => LobbyManager.Instance.QuickJoinLobby());
        }   
    }
}
