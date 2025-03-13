using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MouseOnButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public UnityEvent<PointerEventData> PoinerEnter = new();
    public UnityEvent<PointerEventData> PointerExit = new();
    public UnityEvent<PointerEventData> PointerClick = new();

    public void OnPointerEnter(PointerEventData eventData)
    {
        PoinerEnter?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExit?.Invoke(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PointerClick?.Invoke(eventData);
    }
}