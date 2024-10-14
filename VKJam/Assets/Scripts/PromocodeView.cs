using UnityEngine;
using UnityEngine.UI;

public class PromocodeView : MonoBehaviour
{
    public InputField promocodeInputField; // Ссылка на InputField
    public Button activateButton; // Ссылка на кнопку

    private string promocode;
    private const int MaxPromocodeLength = 10; // Максимальная длина промокода

    void Start()
    {
        // Устанавливаем максимальное количество символов через свойство characterLimit
        promocodeInputField.characterLimit = MaxPromocodeLength;

        // Настраиваем функцию валидации для проверки каждого введённого символа
        promocodeInputField.onValidateInput += ValidatePromocodeInput;

        // Привязываем обработчик нажатия на кнопку
        activateButton.onClick.AddListener(OnActivateButtonClicked);
    }

    // Метод для проверки каждого символа
    private char ValidatePromocodeInput(string text, int charIndex, char addedChar)
    {
        // Допустимы только буквы и цифры
        if (char.IsLetterOrDigit(addedChar))
        {
            return addedChar; // Если символ валиден, возвращаем его
        }
        else
        {
            return '\0'; // Если символ не валиден, возвращаем null-символ (он будет проигнорирован)
        }
    }

    // Обработчик нажатия кнопки
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

    // Функция для активации промокода
    void TryActivatePromocod()
    {
        StartCoroutine(Php_Connect.Request_ActivatePromocod(promocode, OnSuccess, OnFailure));
    }

    // Обработка успешного ответа
    private void OnSuccess()
    {
        Debug.Log("Promocode activated successfully!");
        // Можно добавить логику отображения сообщения об успешной активации
    }

    // Обработка неудачного ответа
    private void OnFailure()
    {
        Debug.Log("Promocode activation failed.");
        // Можно добавить логику отображения сообщения об ошибке
    }
}
