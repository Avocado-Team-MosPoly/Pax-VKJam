using UnityEngine;

public class VKUtility : MonoBehaviour
{
    private void OnEnable()
    {
        //VK_Connect.Executor.OnFriendsGot.AddListener(OnFriendsGot);
        VK_Connect.Instance.OnFriendsGot += OnFriendsGot;
    }

    private void OnDisable()
    {
        //VK_Connect.Executor.OnFriendsGot.RemoveListener(OnFriendsGot);
        VK_Connect.Instance.OnFriendsGot -= OnFriendsGot;
    }

    public void ShowInterstitialAd()
    {
        VK_Connect.Instance.RequestShowInterstitialAd();
    }

    public void JoinGroup()
    {
        VK_Connect.Instance.RequestJoinGroup();
    }

    public void InviteToGame()
    {
        VK_Connect.Instance.RequestInvateNewPlayer();
    }

    public void InviteToLobby()
    {
        if (LobbyManager.Instance.CurrentLobby == null)
            return;

        VK_Connect.Instance.RequestGetFriends();
    }

    private void OnFriendsGot(int[] uids)
    {
        foreach (var uid in uids)
            VK_Connect.Instance.RequestInvateOldPlayer(uid, LobbyManager.Instance.CurrentLobby.LobbyCode);
    }
}