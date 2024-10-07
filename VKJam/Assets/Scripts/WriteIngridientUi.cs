using UnityEngine;
using UnityEngine.UI;
//using DG.Tweening; // ���������� ���������� DOTween ��� ��������

public class WriteIngridientUi : MonoBehaviour
{
    [SerializeField] private InputField inputField; // ��������� ���� ��� �������� ������ �� InputField
    [SerializeField] private Transform uiTransform; // ��������� ���� ��� �������� Transform UI ��������

    public float moveDistance = 100f; // ���������� �������� ��� ��������
    public float animationDuration = 0.5f; // ������������ ��������

    private Vector3 initialPosition; // ��������� ������� ��� ��������������

    private void Awake()
    {
        // ��������� ��������� ������� ��������
        if (uiTransform != null)
        {
            initialPosition = uiTransform.position;
        }
    }

    // ����� ��� ������� ������ � InputField
    public void ClearText()
    {
        if (inputField != null)
        {
            inputField.text = string.Empty;
        }
    }

    // ��������� �����, ������������ ������� ����� �� InputField
    public string GetText()
    {
        return inputField != null ? inputField.text : string.Empty;
    }

    // ��������� �����, ������������ ��� InputField
    public InputField GetInputField()
    {
        return inputField;
    }

    // ����� ��� ������� �������� ������ � ���������� UI
    public void ResetInputField()
    {
        if (inputField != null)
        {
            inputField.text = "";
            inputField.placeholder.GetComponent<Text>().text = "Enter ingredient...";
        }
    }

    // ����� ��� ������ UI � ��������� �������� �����
    public void Show()
    {
        gameObject.SetActive(true); // ���������� ������

        if (uiTransform != null)
        {
            // ������������� ��������� ������� ���� ��������� �� moveDistance �� Y
            //uiTransform.position = new Vector3(initialPosition.x, initialPosition.y - moveDistance, initialPosition.z);

            // ��������� �������� ����� �� moveDistance, ����������� � ��������� �������
            //uiTransform.DOMove(initialPosition, animationDuration).SetEase(Ease.OutQuad);
        }
    }

    // ����� ��� ������� UI � ��������� �������� ����
    public void Hide()
    {
        if (uiTransform != null)
        {
            // �������� ���� � ����� ���������� ����������� �������
            //uiTransform.DOMove(targetPosition, animationDuration).SetEase(Ease.InQuad).OnComplete(() =>
            //{
                gameObject.SetActive(false);
            //});
        }
    }
}


