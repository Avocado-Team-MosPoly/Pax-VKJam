using UnityEngine;
using UnityEngine.Events;

public class SpriteButton : MonoBehaviour
{
    public UnityEvent OnClick;

    private void OnMouseDown()
    {
        OnClick?.Invoke();
    }
}