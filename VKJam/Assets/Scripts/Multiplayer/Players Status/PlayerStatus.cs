using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ulong OwnerClientId { get; private set; }

    [Header("Player status visual")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image checkBoxImage;

    [Header("Checkbox states")]
    [SerializeField] private Sprite checkedSprite;
    [SerializeField] private Sprite uncheckedSprite;

    private string guessStatusText;
    private PlayerStatusDescription statusDescription;

    public PlayerStatus Init(ulong ownerClientId, Sprite avatarImage, Sprite frameImage, string guessStatusText, PlayerStatusDescription statusDescription)
    {
        OwnerClientId = ownerClientId;
        this.avatarImage.sprite = avatarImage;
        this.frameImage.sprite = frameImage;

        checkBoxImage.sprite = uncheckedSprite;
        this.guessStatusText = guessStatusText;
        this.statusDescription = statusDescription;

        return this;
    }

    public void ResetStatus(string defaultStatus)
    {
        guessStatusText = defaultStatus;
        checkBoxImage.sprite = uncheckedSprite;
    }

    public void SetStatus(string guessText)
    {
        guessStatusText = guessText;
        checkBoxImage.sprite = checkedSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        statusDescription.Show(PlayersDataManager.Instance.PlayerDatas[OwnerClientId].Name, guessStatusText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        statusDescription.Hide();
    }
}