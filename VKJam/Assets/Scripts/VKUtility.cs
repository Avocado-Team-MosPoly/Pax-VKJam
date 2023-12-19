using UnityEngine;

public class VKUtility : MonoBehaviour
{
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

        VK_Connect.Executor.OnFriendsGot += (int[] uids) =>
        {
            foreach(var uid in uids)
            {
                VK_Connect.Executor.RequestInvateOldPlayer(uid, LobbyManager.Instance.CurrentLobby.LobbyCode);
            }
        };

        VK_Connect.Executor.RequestGetFriends();
    }
}
