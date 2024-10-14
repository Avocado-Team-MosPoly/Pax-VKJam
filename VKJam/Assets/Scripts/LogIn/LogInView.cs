using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogInView : MonoBehaviour
{
    /// <summary> login, password </summary>
    public event Action<string, string> Submitted;

    [SerializeField] private TMP_InputField loginInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    [SerializeField] private Button submitButton;

    private void OnEnable()
    {
        submitButton.onClick.AddListener(Submit);
    }

    private void OnDisable()
    {
        submitButton.onClick.RemoveListener(Submit);
    }

    private void Submit()
    {
        if (string.IsNullOrEmpty(loginInputField.text) ||
            string.IsNullOrEmpty(passwordInputField.text))
            return;

        Submitted?.Invoke(loginInputField.text, passwordInputField.text);
    }

    public void ShowLoginError()
    {

    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}