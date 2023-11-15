using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class InteractableTM : MonoBehaviour
{
    [SerializeField] private Material[] TargetMaterialID;
    private Color[] TargetColorMaterial;
    [SerializeField] private Color NewColor;
    private Renderer Rend;
    public UnityEvent m_OnClick;
    public UnityEvent m_OnMouseEnter;
    public UnityEvent m_OnMouseExit;
    public bool ActivityInteractable;
    
    private void Awake()
    {
        Rend = GetComponent<Renderer>();
        TargetColorMaterial = new Color[TargetMaterialID.Length];
        for(int i =0;i < TargetMaterialID.Length;++i)
            TargetColorMaterial[i] = TargetMaterialID[i].color;
    }
    void OnMouseEnter()
    {
        if (!ActivityInteractable) return;
        m_OnMouseEnter.Invoke();
        foreach (var current in TargetMaterialID)
            current.color = NewColor;
    }
    void OnMouseExit()
    {
        if (!ActivityInteractable) return;
        m_OnMouseExit.Invoke();
        for (int i = 0; i < TargetMaterialID.Length; ++i)
            TargetMaterialID[i].color = TargetColorMaterial[i];
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
