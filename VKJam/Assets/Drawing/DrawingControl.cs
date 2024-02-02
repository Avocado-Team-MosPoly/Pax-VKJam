using UnityEngine;
using UnityEngine.UI;

public class DrawingControl : MonoBehaviour
{
    [SerializeField] private DrawingMultiplayer drawing;
    [SerializeField] private DrawingWithMeshModification drawingwmm;

    [SerializeField] private Slider brushSizeSlider;

    private void Start()
    {
        brushSizeSlider.onValueChanged.AddListener(ChangeBrushSize);
    }

    public void ChangeBrushSize(float value)
    {
        if (drawing)
            drawing.BrushSize = value;
        if (drawingwmm)
            drawingwmm.BrushSize = value;
    }
}