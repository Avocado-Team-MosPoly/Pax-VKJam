using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.UI;


public class LeaderboardManager : NetworkBehaviour
{
    [SerializeField] private Image winnerAvatar;
    [SerializeField] private Image winnerFrame;
    [SerializeField] private TMP_Text winnerName;
    [SerializeField] private TMP_Text winnerScore;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform entriesContainer;

    public void ShowLeaderboard(Dictionary<ulong, int> playerScores)
    {
        ClearLeaderboard();

        ulong winnerId = 0;
        int maxScore = -1;

        foreach (var entry in playerScores)
            if (entry.Value > maxScore)
            {
                maxScore = entry.Value;
                winnerId = entry.Key;
            }

        if (PlayersDataManager.Instance.PlayersData.TryGetValue(winnerId, out PlayerData winnerData))
        {
            var store = PlayersDataManager.Instance.AvatarsAndFramesStorage;

            Sprite winnerAvatarSprite = Resources.Load<Sprite>(store.products[winnerData.AvatarIndex].Data.icon);
            Sprite winnerFrameSprite = Resources.Load<Sprite>(store.products[winnerData.AvatarFrameIndex].Data.icon);

            winnerAvatar.sprite = winnerAvatarSprite;
            winnerFrame.sprite = winnerFrameSprite;
            winnerName.text = winnerData.Name;
            winnerScore.text = playerScores[winnerId].ToString();
        }

        foreach (var player in PlayersDataManager.Instance.PlayersData)
        {
            if (player.Key == winnerId) continue;

            if (playerScores.TryGetValue(player.Key, out int scoreData))
            {
                CreatePlayerEntry(player.Value, scoreData);
            }
        }
    }

    private void CreatePlayerEntry(PlayerData playerData, int scoreData)
    {
        var entry = Instantiate(playerPrefab, entriesContainer);
        var store = PlayersDataManager.Instance.AvatarsAndFramesStorage;

        Sprite avatarSprite = Resources.Load<Sprite>(store.products[playerData.AvatarIndex].Data.icon);
        Sprite frameSprite = Resources.Load<Sprite>(store.products[playerData.AvatarFrameIndex].Data.icon);

        entry.GetComponent<PlayerPrefabUI>().Initialize(
            avatar: avatarSprite,
            frame: frameSprite,
            playerName: playerData.Name,
            score: scoreData.ToString()
        );
    }

    private void ClearLeaderboard()
    {
        foreach (Transform child in entriesContainer)
            Destroy(child.gameObject);

        winnerAvatar.sprite = null;
        winnerFrame.sprite = null;
        winnerName.text = "";
        winnerScore.text = "";
    }

}