using UnityEngine;
using UnityEngine.UI;
//using DG.Tweening; // Подключаем библиотеку DOTween для анимаций

public class WriteIngridientUi : MonoBehaviour
{
    [SerializeField] private InputField inputField; // Приватное поле для хранения ссылки на InputField
    [SerializeField] private Transform uiTransform; // Приватное поле для хранения Transform UI элемента

    public float moveDistance = 100f; // Расстояние движения для анимации
    public float animationDuration = 0.5f; // Длительность анимации

    private Vector3 initialPosition; // Начальная позиция для восстановления

    private void Awake()
    {
        // Сохраняем стартовую позицию элемента
        if (uiTransform != null)
        {
            initialPosition = uiTransform.position;
        }
    }

    // Метод для очистки текста в InputField
    public void ClearText()
    {
        if (inputField != null)
        {
            inputField.text = string.Empty;
        }
    }

    // Публичный метод, возвращающий текущий текст из InputField
    public string GetText()
    {
        return inputField != null ? inputField.text : string.Empty;
    }

    // Публичный метод, возвращающий сам InputField
    public InputField GetInputField()
    {
        return inputField;
    }

    // Метод для полного удаления текста и обновления UI
    public void ResetInputField()
    {
        if (inputField != null)
        {
            inputField.text = "";
            inputField.placeholder.GetComponent<Text>().text = "Enter ingredient...";
        }
    }

    // Метод для показа UI с анимацией движения вверх
    public void Show()
    {
        gameObject.SetActive(true); // Активируем объект

        if (uiTransform != null)
        {
            // Устанавливаем начальную позицию ниже стартовой на moveDistance по Y
            //uiTransform.position = new Vector3(initialPosition.x, initialPosition.y - moveDistance, initialPosition.z);

            // Анимируем движение вверх на moveDistance, возвращаясь к начальной позиции
            //uiTransform.DOMove(initialPosition, animationDuration).SetEase(Ease.OutQuad);
        }
    }

    // Метод для скрытия UI с анимацией движения вниз
    public void Hide()
    {
        if (uiTransform != null)
        {
            // Анимация вниз и после завершения деактивация объекта
            //uiTransform.DOMove(targetPosition, animationDuration).SetEase(Ease.InQuad).OnComplete(() =>
            //{
                gameObject.SetActive(false);
            //});
        }
    }
}


