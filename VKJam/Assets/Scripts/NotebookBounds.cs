using UnityEngine;
using UnityEngine.Events;

public class NotebookBounds : MonoBehaviour
{
    [SerializeField] private Material boundsMaterial;

    [SerializeField] private Texture defaultTexture;
    [SerializeField] private Texture holdHintTexture;

    [HideInInspector] public UnityEvent OnEnabled = new();
    [HideInInspector] public UnityEvent OnDisabled = new();

    private void OnEnable()
    {
        OnEnabled.Invoke();
    }

    private void OnDisable()
    {
        OnDisabled.Invoke();
    }

    public void SetDefaultTexture()
    {
        boundsMaterial.mainTexture = defaultTexture;
    }

    public void SetHoldHintTexture()
    {
        boundsMaterial.mainTexture = holdHintTexture;
    }
}