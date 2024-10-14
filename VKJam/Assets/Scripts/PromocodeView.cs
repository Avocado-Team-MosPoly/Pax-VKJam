using UnityEngine;
using UnityEngine.UI;

public class PromocodeView : MonoBehaviour
{
    public InputField promocodeInputField; // ������ �� InputField
    public Button activateButton; // ������ �� ������

    private string promocode;
    private const int MaxPromocodeLength = 10; // ������������ ����� ���������

    void Start()
    {
        // ������������� ������������ ���������� �������� ����� �������� characterLimit
        promocodeInputField.characterLimit = MaxPromocodeLength;

        // ����������� ������� ��������� ��� �������� ������� ��������� �������
        promocodeInputField.onValidateInput += ValidatePromocodeInput;

        // ����������� ���������� ������� �� ������
        activateButton.onClick.AddListener(OnActivateButtonClicked);
    }

    // ����� ��� �������� ������� �������
    private char ValidatePromocodeInput(string text, int charIndex, char addedChar)
    {
        // ��������� ������ ����� � �����
        if (char.IsLetterOrDigit(addedChar))
        {
            return addedChar; // ���� ������ �������, ���������� ���
        }
        else
        {
            return '\0'; // ���� ������ �� �������, ���������� null-������ (�� ����� ��������������)
        }
    }

    // ���������� ������� ������
    private void OnActivateButtonClicked()
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

    // ������� ��� ��������� ���������
    void TryActivatePromocod()
    {
        StartCoroutine(Php_Connect.Request_ActivatePromocod(promocode, OnSuccess, OnFailure));
    }

    // ��������� ��������� ������
    private void OnSuccess()
    {
        Debug.Log("Promocode activated successfully!");
        // ����� �������� ������ ����������� ��������� �� �������� ���������
    }

    // ��������� ���������� ������
    private void OnFailure()
    {
        Debug.Log("Promocode activation failed.");
        // ����� �������� ������ ����������� ��������� �� ������
    }
}
