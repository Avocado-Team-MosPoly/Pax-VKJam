using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaintController : MonoBehaviour
{
    [SerializeField] private Paint paint;

    [SerializeField, Range(0, 15)] private int scrollRate = 2;

    [SerializeField] private TextMeshProUGUI sizeInfo;
    [SerializeField] private Slider sizeSlider;

    private byte maxBrushSize = 16;
    private byte minBrushSize = 4;
    private int localBrushSize;
    private float timer = 0.5f;
    private bool canSet;
    private Coroutine canSetCoroutine;

    private void Start()
    {
        localBrushSize = paint.BrushSize;
        
        sizeSlider.minValue = minBrushSize;
        sizeSlider.maxValue = maxBrushSize;
        sizeSlider.value = localBrushSize;

        sizeSlider.wholeNumbers = true;

        sizeSlider.onValueChanged.AddListener(ChangeBrushSize);

        sizeInfo.text = localBrushSize.ToString();
    }

    private void Update()
    {
        Scroll();
    }

    private void Scroll()
    {
        int scrollDelta = (int)Input.mouseScrollDelta.y;
        
        if (scrollDelta != 0)
        {
            localBrushSize += scrollRate * scrollDelta;
        }
        else if (localBrushSize != paint.BrushSize)
        {
            localBrushSize = Mathf.Clamp(localBrushSize, minBrushSize, maxBrushSize);
            if (localBrushSize != paint.BrushSize)
            {
                if (canSet)
                {
                    paint.ChangeSize((byte)localBrushSize);
                    canSet = false;
                }
                else if (canSetCoroutine == null)
                {
                    canSetCoroutine = StartCoroutine(ScrollLock());
                }
            }
        }
    }

    public void ChangeBrushSize(float value)
    {
        sizeInfo.text = value.ToString();

        int scrollDelta = (int)value - paint.BrushSize;

        if (scrollDelta != 0)
        {
            scrollDelta = Mathf.Clamp(scrollDelta - scrollDelta % 2, minBrushSize, maxBrushSize);
            localBrushSize += scrollDelta;
            
            if (canSetCoroutine != null)
            {
                StopCoroutine(canSetCoroutine);
                canSetCoroutine = null;
            }
        }
        else if (localBrushSize != paint.BrushSize)
        {
            localBrushSize = Mathf.Clamp(localBrushSize, minBrushSize, maxBrushSize);
            if (localBrushSize != paint.BrushSize)
            {
                if (canSet)
                {
                    paint.ChangeSize((byte)localBrushSize);
                    canSet = false;
                }
                else if (canSetCoroutine == null)
                {
                    canSetCoroutine = StartCoroutine(ScrollLock());
                }
            }
        }
    }

    private IEnumerator ScrollLock()
    {
        yield return new WaitForSeconds(timer);

        canSet = true;
        canSetCoroutine = null;
    }
}