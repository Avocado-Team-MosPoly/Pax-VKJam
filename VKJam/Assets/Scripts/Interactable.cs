using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] private int TargetMaterialID = 0;
    public UnityEvent m_OnClick;
    void OnMouseEnter()
    {
        GetComponent<Renderer>().materials[TargetMaterialID].color = Color.green; 
    }
    void OnMouseExit()
    {
        GetComponent<Renderer>().materials[TargetMaterialID].color = Color.white;
    }
    void OnMouseDown()
    {
            m_OnClick.Invoke();
    }
}
