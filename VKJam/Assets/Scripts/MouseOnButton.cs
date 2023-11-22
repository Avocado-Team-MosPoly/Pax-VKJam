using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOnButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private GameObject hover;
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        hover.SetActive(true);
        hover.transform.position = eventData.position;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        hover.SetActive(false);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {

    }
    

}
