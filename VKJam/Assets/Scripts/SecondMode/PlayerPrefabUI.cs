using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerPrefabUI : MonoBehaviour
{
    [SerializeField] private Image avatarImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text scoreText;

    public void Initialize(Sprite avatar, Sprite frame, string playerName, string score)
    {
        avatarImage.sprite = avatar;
        frameImage.sprite = frame;
        playerNameText.text = playerName;
        scoreText.text = score;
    }
}