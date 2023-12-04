using UnityEngine;
using UnityEngine.Events;
//using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    public int[] TargetMaterialID;
    private Color[] TargetColorMaterial;
    public Color NewColor;
    private Renderer Rend;
    public UnityEvent m_OnClick;
    public UnityEvent m_OnMouseEnter;
    public UnityEvent m_OnMouseExit;
    public bool ActivityInteractable;

    //private bool isMouseEntered;

    public void Set(Interactable New)
    {
        TargetMaterialID = New.TargetMaterialID;
        NewColor = New.NewColor;
        m_OnClick = New.m_OnClick;
        m_OnMouseEnter = New.m_OnMouseEnter;
        m_OnMouseExit = New.m_OnMouseExit;
        ActivityInteractable = New.ActivityInteractable;
    }
    private void Awake()
    {
        Rend = GetComponent<Renderer>();
        TargetColorMaterial = new Color[TargetMaterialID.Length];
        for(int i =0;i < TargetMaterialID.Length;++i)
            TargetColorMaterial[i] = Rend.materials[TargetMaterialID[i]].color;
    }
    void OnMouseEnter()
    {
        if (!ActivityInteractable/* || EventSystem.current.IsPointerOverGameObject()*/)
            return;

        //isMouseEntered = true;
        m_OnMouseEnter.Invoke();
        foreach (int current in TargetMaterialID)
            Rend.materials[current].color = NewColor;
    }
    void OnMouseExit()
    {
        if (!ActivityInteractable/* || !isMouseEntered*/)
            return;

        //isMouseEntered = false;
        m_OnMouseExit.Invoke();
        for (int i = 0; i < TargetMaterialID.Length; ++i)
            Rend.materials[TargetMaterialID[i]].color = TargetColorMaterial[i];
    }
    void OnMouseDown()
    {
        if (!ActivityInteractable/* || EventSystem.current.IsPointerOverGameObject()*/)
            return;

        m_OnClick.Invoke();
    }

    public void SetInteractable(bool check)
    {
       ActivityInteractable = check;
    }

}
