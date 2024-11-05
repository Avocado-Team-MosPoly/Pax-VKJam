using TMPro;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Vector3 positionOffset;

    private RectTransform rectTransform;

    private void Start()
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

        rectTransform.anchoredPosition = newPosition;
    }

    public void Show(string text)
    {
        gameObject.SetActive(true);
        label.text = text;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}