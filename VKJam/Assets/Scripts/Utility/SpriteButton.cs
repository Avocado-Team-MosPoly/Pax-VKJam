using UnityEngine;
using UnityEngine.Events;

public class SpriteButton : MonoBehaviour
{
    public UnityEvent OnClick = new();

    private void OnMouseDown()
    {
        OnClick?.Invoke();
    }
}