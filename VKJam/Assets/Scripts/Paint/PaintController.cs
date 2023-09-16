using System.Collections;
using UnityEngine;

public class PaintController : MonoBehaviour
{
    [SerializeField] private Paint paint;

    [SerializeField, Range(0, 15)] private int scrollRate = 2;
    private byte maxBrushSize = 16;
    private byte minBrushSize = 4;
    private int localBrushSize;
    private float timer = 0.5f;
    private bool canSet;
    private Coroutine canSetCoroutine;

    private void Start()
    {
        localBrushSize = paint.BrushSize;
    }

    private void Update()
    {
        Scroll();
    }

    private void Scroll()
    {
        int scrollDelta = (int)Input.mouseScrollDelta.y;
        
        if (scrollDelta != 0f)
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

    private IEnumerator ScrollLock()
    {
        yield return new WaitForSeconds(timer);

        canSet = true;
        canSetCoroutine = null;
    }
}