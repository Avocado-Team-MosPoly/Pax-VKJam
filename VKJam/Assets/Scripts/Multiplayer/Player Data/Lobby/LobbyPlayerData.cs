using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyPlayerDataView : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [HideInInspector] public UnityEvent<LobbyPlayerDataView> PointerEntered = new();
    [HideInInspector] public UnityEvent<LobbyPlayerDataView> PointerExit = new();
    [HideInInspector] public UnityEvent<LobbyPlayerDataView> Clicked = new();

    public ulong ClientId { get; private set; }
    public string ClientName { get; private set; }

    [SerializeField] private Image avatarImage;
    [SerializeField] private Image frameImage;

    [SerializeField] private Color selectedFrameColor = new(186f / 255f, 178f / 255f, 116f / 255f);

    private Color defaultFrameColor = Color.white;

    private void SearchPlayerName()
    {
        if (!string.IsNullOrEmpty(ClientName))
            return;

        if (PlayersDataManager.Instance.PlayersData.ContainsKey(ClientId))
            ClientName = PlayersDataManager.Instance.PlayersData[ClientId].Name;
    }

    public void SetData(string name, ulong clientId)
    {
        ClientName = name;
        ClientId = clientId;
    }

    public void SetAvatar(Sprite avatar)
    {
        avatarImage.sprite = avatar;
    }

    public void SetFrame(Sprite frame)
    {
        frameImage.sprite = frame;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SearchPlayerName();
        frameImage.color = selectedFrameColor;
        PointerEntered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        frameImage.color = defaultFrameColor;
        PointerExit?.Invoke(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Clicked?.Invoke(this);
    }
}