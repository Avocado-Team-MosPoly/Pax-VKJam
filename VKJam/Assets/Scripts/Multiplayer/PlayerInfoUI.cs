using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;

    public void UpdatePlayer(Player player)
    {
        playerName.text = player.Data["Player Name"].Value;
    }
}