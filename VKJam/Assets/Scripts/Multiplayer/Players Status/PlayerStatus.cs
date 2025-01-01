using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Player status visual")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image checkBoxImage;

    [Header("Checkbox states")]
    [SerializeField] private Sprite checkedSprite;
    [SerializeField] private Sprite uncheckedSprite;

    private PlayerStatusDescription statusDescription;

    public ulong OwnerClientId { get; private set; }
    public string GuessStatusText { get; private set; }

    public void Init(ulong ownerClientId, Sprite avatarImage, Sprite frameImage, string guessStatusText, PlayerStatusDescription statusDescription)
    {
        OwnerClientId = ownerClientId;
        this.avatarImage.sprite = avatarImage;
        this.frameImage.sprite = frameImage;

        checkBoxImage.sprite = uncheckedSprite;
        GuessStatusText = guessStatusText;
        this.statusDescription = statusDescription;
    }

    public void ResetStatus(string defaultStatus = "")
    {
        GuessStatusText = defaultStatus;
        checkBoxImage.sprite = uncheckedSprite;
    }

    public void SetStatus(string guessText)
    {
        GuessStatusText = guessText;
        checkBoxImage.sprite = checkedSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        statusDescription.Show(PlayersDataManager.Instance.PlayerDatas[OwnerClientId].Name, GuessStatusText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        statusDescription.Hide();
    }
}