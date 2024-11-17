using UnityEngine;
using UnityEngine.UI;

public class PromocodeView : MonoBehaviour
{
    public InputField promocodeInputField; 
    public Button activateButton; 

    private string promocode;
    private const int MaxPromocodeLength = 10; 

    void Start()
    {
        promocodeInputField.characterLimit = MaxPromocodeLength;

        promocodeInputField.onValidateInput += ValidatePromocodeInput;

        activateButton.onClick.AddListener(ActivateButton);
    }

    private char ValidatePromocodeInput(string text, int charIndex, char addedChar)
    {
        if (char.IsLetterOrDigit(addedChar))
        {
            return addedChar; 
        }
        else
        {
            return '\0';
        }
    }

    private void ActivateButton()
    {
        promocode = promocodeInputField.text;

        if (!string.IsNullOrEmpty(promocode))
        {
            TryActivatePromocod();
        }
        else
        {
            Debug.Log("Promocode is empty.");
        }
    }

    void TryActivatePromocod()
    {
        StartCoroutine(Php_Connect.Request_ActivatePromocod(promocode, OnSuccess, OnFailure));
    }

    private void OnSuccess()
    {
        Debug.Log("Promocode activated successfully!");
        //вывести пользователю ответ
    }

    private void OnFailure()
    {
        Debug.Log("Promocode activation failed.");
    }
}
