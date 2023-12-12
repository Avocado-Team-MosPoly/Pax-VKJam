using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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
    public bool ActivityInteractable = true;
    public bool EnableMaterialSwitch = true;

    private bool isMouseEntered;

    public void Set(Interactable New)
    {
        InitTargetMaterials();

        //if(New.TargetMaterialID != null) TargetMaterialID = New.TargetMaterialID;
        if (New.NewColor != null) NewColor = New.NewColor;
        if (New.m_OnClick != null) m_OnClick = New.m_OnClick;
        if (New.m_OnMouseEnter != null) m_OnMouseEnter = New.m_OnMouseEnter;
        if (New.m_OnMouseExit != null) m_OnMouseExit = New.m_OnMouseExit;
        ActivityInteractable = New.ActivityInteractable;
    }

    private void Awake()
    {
        InitTargetMaterials();
    }

    private void InitTargetMaterials()
    {
        if (Rend == null && !TryGetComponent(out Rend))
        {
            Logger.Instance.LogWarning(this, $"GameObject doesn't contain Renderer component");
            return;
        }
        else if (!EnableMaterialSwitch)
            return;

        TargetMaterialID = new int[Rend.materials.Length];
        for (int i = 0; i < Rend.materials.Length; i++)
            TargetMaterialID[i] = i;

        TargetColorMaterial = new Color[TargetMaterialID.Length];
        for (int i = 0; i < TargetColorMaterial.Length; i++)
            TargetColorMaterial[i] = Rend.materials[TargetMaterialID[i]].color;
    }

    void OnMouseEnter()
    {
        if (!ActivityInteractable || EventSystem.current.IsPointerOverGameObject())
            return;

        isMouseEntered = true;
        m_OnMouseEnter.Invoke();

        if(EnableMaterialSwitch)
            foreach (int current in TargetMaterialID)
                Rend.materials[current].color = NewColor;
    }
    void OnMouseExit()
    {
        if (!ActivityInteractable || !isMouseEntered)
            return;

        isMouseEntered = false;
        m_OnMouseExit.Invoke();

        if (EnableMaterialSwitch)
            for (int i = 0; i < TargetMaterialID.Length; i++)
                Rend.materials[TargetMaterialID[i]].color = TargetColorMaterial[i];
    }
    void OnMouseDown()
    {
        if (!ActivityInteractable || EventSystem.current.IsPointerOverGameObject())
            return;

        m_OnClick.Invoke();
    }

    public void SetInteractable(bool check)
    {
       ActivityInteractable = check;
    }

}
