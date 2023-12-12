using UnityEngine;
using UnityEngine.Events;

public class NotebookBounds : MonoBehaviour
{
    [SerializeField] private Material boundsMaterial;

    [SerializeField] private Texture defaultTexture;
    [SerializeField] private Texture holdHintTexture;

    public void SetDefaultTexture()
    {
        boundsMaterial.mainTexture = defaultTexture;
    }

    public void SetHoldHintTexture()
    {
        boundsMaterial.mainTexture = holdHintTexture;
    }
}