using TMPro;
using UnityEngine;

public class PlayerStatusDescription : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI statusLabel;

    [SerializeField] private Vector3 positionOffset = new(15f, 0f, 0f);

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = Vector2.zero;
    }

    private void Update()
    {
        MoveToMousePosition();
    }

    private void MoveToMousePosition()
    {
        Vector3 newPosition = Input.mousePosition + positionOffset;

        if (newPosition.x + rectTransform.sizeDelta.x > Screen.width)
            newPosition.x = Input.mousePosition.x - positionOffset.x - rectTransform.sizeDelta.x;
        if (newPosition.y + rectTransform.sizeDelta.y > Screen.height)
            newPosition.y = Input.mousePosition.y - positionOffset.y - rectTransform.sizeDelta.y;

        transform.position = newPosition;
    }

    public void Show(string clientName, string status)
    {
        nameLabel.text = clientName;
        statusLabel.text = status;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}