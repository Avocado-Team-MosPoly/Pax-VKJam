using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonSet<T> : MonoBehaviour
{
    [Serializable] public struct ButtonValue
    {
        public readonly static ButtonValue Null = new ButtonValue
        {
            Button = null,
            Value = default
        };

        public MouseOnButton Button;
        public T Value;
    }

    [SerializeField] private ButtonValue[] buttonValues;

    [SerializeField] private Transform hoverSelected;
    [SerializeField] private Transform hoverClicked;

    public readonly UnityEvent<T> OnClick = new();

    private ButtonValue clickedButtonValue;

    private void Start()
    {
        hoverSelected.gameObject.SetActive(false);

        hoverClicked.transform.position = buttonValues[0].Button.transform.position;
        hoverClicked.gameObject.SetActive(true);

        OnClick?.Invoke(buttonValues[0].Value);

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
            buttonValue.Button.PoinerEnter.AddListener(Enter);
            buttonValue.Button.PointerExit.AddListener(Exit);
            buttonValue.Button.PointerClick.AddListener(Click);
        }
    }

    private ButtonValue GetButtonValue(MouseOnButton mouseOnButton)
    {
        foreach (ButtonValue buttonValue in buttonValues)
            if (buttonValue.Button == mouseOnButton)
                return buttonValue;

        return new();
    }

    private void Enter(PointerEventData eventData)
    {
        if (eventData == null || !eventData.pointerEnter.TryGetComponent<MouseOnButton>(out var mouseOnButton))
            return;

        ButtonValue buttonValue = GetButtonValue(mouseOnButton);

        if (buttonValue.Equals(ButtonValue.Null) || buttonValue.Equals(clickedButtonValue))
            return;

        hoverSelected.position = mouseOnButton.transform.position;
        hoverSelected.gameObject.SetActive(true);
    }

    private void Exit(PointerEventData eventData)
    {
        if (eventData == null || !eventData.pointerEnter.TryGetComponent<MouseOnButton>(out var mouseOnButton))
            return;

        hoverSelected.gameObject.SetActive(false);
    }

    private void Click(PointerEventData eventData)
    {
        if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!eventData.pointerEnter.TryGetComponent<MouseOnButton>(out var mouseOnButton))
            return;

        ButtonValue buttonValue = GetButtonValue(mouseOnButton);

        if (buttonValue.Equals(ButtonValue.Null))
            return;

        clickedButtonValue = buttonValue;
        hoverClicked.position = clickedButtonValue.Button.transform.position;
        OnClick?.Invoke(buttonValue.Value);
    }
}