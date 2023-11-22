using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonSet<T> : MonoBehaviour
{
    [Serializable] public struct ButtonValue
    {
        public Button Button;
        public T Value;
    }

    [SerializeField] private ButtonValue[] buttonValues;

    [SerializeField] private GameObject hoverSelected;

    public readonly UnityEvent<T> OnClick = new();

    private ButtonValue selectedButtonValue;

    private void Start()
    {
        InitButtons();
    }

    private void InitButtons()
    {
        if (buttonValues.Length <= 0)
        {
            Debug.LogWarning("Buttons are not set");
            return;
        }

        foreach (ButtonValue buttonValue in buttonValues)
        {
            buttonValue.Button.onClick.AddListener(() => Click(buttonValue));
        }

        Click(buttonValues[0]);
    }

    private void Click(ButtonValue buttonValue)
    {
        selectedButtonValue = buttonValue;
        hoverSelected.transform.position= selectedButtonValue.Button.transform.position;
        OnClick?.Invoke(buttonValue.Value);
    }
}