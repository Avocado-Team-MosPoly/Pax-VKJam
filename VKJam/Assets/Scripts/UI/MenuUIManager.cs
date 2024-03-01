using UnityEngine;

public class MenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject joinVKGroupButton;

    private void Start()
    {
        // Join VK group button set up
        OnIsJoinedVKGroupValueChanged(false);
        VK_Connect.Instance.IsJoinedVKGroupObserver.ValueChanged += OnIsJoinedVKGroupValueChanged;
        VK_Connect.Instance.RequestCheckSubscriptionVKGroup();
    }

    private void OnDestroy()
    {
        VK_Connect.Instance.IsJoinedVKGroupObserver.ValueChanged -= OnIsJoinedVKGroupValueChanged;
    }

    private void OnIsJoinedVKGroupValueChanged(bool isJoined)
    {
        joinVKGroupButton.SetActive(!isJoined);
    }
}