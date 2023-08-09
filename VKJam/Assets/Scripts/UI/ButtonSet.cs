using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ButtonSet<T> : MonoBehaviour
{
    [Serializable] public struct ButtonValue
    {
        public Button Button;
        public T Value;
    }

    [SerializeField] private ButtonValue[] buttonValues;

    [SerializeField] private Sprite defaultState;
    [SerializeField] private Sprite clickedState;

    public readonly UnityEvent<T> OnClick = new();

    private ButtonValue selectedButtonValue;

    private void Awake()
    {
        InitButtons();
    }

    private void InitButtons()
    {
        foreach (ButtonValue buttonValue in buttonValues)
            buttonValue.Button.onClick.AddListener( () => Click(buttonValue) );
    }

    private void Click(ButtonValue buttonValue)
    {
        if (selectedButtonValue.Button != null)
            selectedButtonValue.Button.GetComponent<Image>().sprite = defaultState;

        selectedButtonValue = buttonValue;

        selectedButtonValue.Button.GetComponent<Image>().sprite = clickedState;

        OnClick?.Invoke(buttonValue.Value);
    }
}