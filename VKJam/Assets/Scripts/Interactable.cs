using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [SerializeField] private int[] TargetMaterialID;
    [SerializeField] private Color NewColor;
    private Renderer Rend;
    public UnityEvent m_OnClick;
    private void Awake()
    {
        Rend = GetComponent<Renderer>();
    }
    void OnMouseEnter()
    {
        foreach (int current in TargetMaterialID)
            Rend.materials[current].color = NewColor;
    }
    void OnMouseExit()
    {
        foreach (int current in TargetMaterialID)
            Rend.materials[current].color = Color.white;
    }
    void OnMouseDown()
    {
        m_OnClick.Invoke();
    }
}