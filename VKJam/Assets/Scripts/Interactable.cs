using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [SerializeField] private int[] TargetMaterialID;
    [SerializeField] private Color NewColor;
    private Renderer Rend;
    public UnityEvent m_OnClick;
    public UnityEvent m_OnHover;
    public bool ActivityInteractable;
    
    private void Awake()
    {
        Rend = GetComponent<Renderer>();
    }
    void OnMouseEnter()
    {
        if (!ActivityInteractable) return;
        m_OnHover.Invoke();
        foreach (int current in TargetMaterialID)
            Rend.materials[current].color = NewColor;
    }
    void OnMouseExit()
    {
        if (!ActivityInteractable) return;
        foreach (int current in TargetMaterialID)
            Rend.materials[current].color = Color.white;
    }
    void OnMouseDown()
    {
        if (!ActivityInteractable) return;
        m_OnClick.Invoke();
    }

    public void SetInteractable(bool check)
    {
       ActivityInteractable = check;
    }

}