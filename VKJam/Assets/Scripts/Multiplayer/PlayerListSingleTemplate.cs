using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerListSingleTemplate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;

    public void UpdatePlayer(Player player)
    {
        playerName.text = player.Data["Player Name"].Value;
    }
}
