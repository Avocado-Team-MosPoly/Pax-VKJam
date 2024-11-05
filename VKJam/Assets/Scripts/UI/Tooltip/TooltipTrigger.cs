using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, TextArea(1, 10)] private string tooltipText;
    [SerializeField] private Tooltip tooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.Show(tooltipText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.Hide();
    }
}