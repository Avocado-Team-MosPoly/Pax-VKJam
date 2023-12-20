using UnityEngine;

public class VKUtility : MonoBehaviour
{
    private void OnEnable()
    {
        //VK_Connect.Executor.OnFriendsGot.AddListener(OnFriendsGot);
        VK_Connect.Executor.OnFriendsGot += OnFriendsGot;
    }

    private void OnDisable()
    {
        //VK_Connect.Executor.OnFriendsGot.RemoveListener(OnFriendsGot);
        VK_Connect.Executor.OnFriendsGot -= OnFriendsGot;
    }

    public void JoinGroup()
    {
        VK_Connect.Executor.RequestJoinGroup();
    }

    public void InviteToGame()
    {
        VK_Connect.Executor.RequestInvateNewPlayer();
    }

    public void InviteToLobby()
    {
        if (LobbyManager.Instance.CurrentLobby == null)
            return;

        VK_Connect.Executor.RequestGetFriends();
    }

    private void OnFriendsGot(int[] uids)
    {
        foreach (var uid in uids)
            VK_Connect.Executor.RequestInvateOldPlayer(uid, LobbyManager.Instance.CurrentLobby.LobbyCode);
    }
}