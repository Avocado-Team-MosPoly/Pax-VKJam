using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MouseOnButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public UnityEvent<PointerEventData> PoinerEnter;
    public UnityEvent<PointerEventData> PointerExit;
    public UnityEvent<PointerEventData> PointerClick;

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