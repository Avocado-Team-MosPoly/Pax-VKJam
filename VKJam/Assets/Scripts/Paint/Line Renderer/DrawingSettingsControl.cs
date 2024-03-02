using System;
using UnityEngine;

public class DrawingSettingsControl : MonoBehaviour
{
    [SerializeField] private DrawingMultiplayer drawing;

    [Header("Brush Size")]
    [SerializeField, Min(0f)] private float minSize = 0.5f;
    [SerializeField, Min(0f)] private float maxSize = 5f;
    [SerializeField] private float sizeLerpRate = 0.5f;
    [SerializeField] private float minSizeDiff = 0.25f;
    [SerializeField, Tooltip("Time in seconds")] private float sizeConfirmationTime = 0.5f;

    private float brushSizeBuffer;
    private Action onSizeConfirmationTimeExpired;

    private Timer timer;

    private void OnValidate()
    {
        if (minSize > maxSize)
        {
            Debug.LogError($"\"Min Size({minSize})\" must be less than or equals \"Max Size({maxSize})\"");
            (minSize, maxSize) = (maxSize, minSize);
        }

        if (sizeLerpRate < minSizeDiff)
            Debug.LogError($"\"Size Lerp({sizeLerpRate})\" Rate must be more than or equals \"Min Diff({minSizeDiff})\"");
    }

    private void Awake()
    {
        OnValidate();

        timer = gameObject.AddComponent<Timer>();

        brushSizeBuffer = drawing.BrushSize;
        onSizeConfirmationTimeExpired = () => drawing.SetBrushSize(brushSizeBuffer);
    }

    private void Update()
    {
        BrushSizeCalculate();
    }

    public void BrushSizeCalculate()
    {
        float mouseScrollDelta = Input.mouseScrollDelta.y * sizeLerpRate;
        float newBrushSize = brushSizeBuffer + mouseScrollDelta;

        if (newBrushSize >= minSize && newBrushSize <= maxSize && Mathf.Abs(brushSizeBuffer - newBrushSize) >= minSizeDiff)
        {
            brushSizeBuffer = newBrushSize;

            if (timer.IsExpired)
                timer.StartTimer(sizeConfirmationTime, onSizeConfirmationTimeExpired);
        }
    }
}