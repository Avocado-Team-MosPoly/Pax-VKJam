using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonsSet : MonoBehaviour
{
    [SerializeField] private int minimumValue;

    [SerializeField] private Button[] buttons;

    public UnityEvent<int> OnClick = new();

    private void Awake()
    {
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(Click);
        }
    }

    private void Click()
    {
        
    }
}