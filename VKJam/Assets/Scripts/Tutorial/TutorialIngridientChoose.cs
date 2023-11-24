using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialIngridientChoose : MonoBehaviour
{
    [SerializeField] private bool isLastIngredient;
    [SerializeField] private bool isCorrect;
    [SerializeField] private GameObject IngridientModel = null;
    [SerializeField] private Button closeBestiaryButton;

    private int _correctAnswers = 0;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(CheckIngridient);
    }

    public void SetIngridient(bool currentIngridient)
    {
        isCorrect = currentIngridient;
    }

    public void CheckIngridient()
    {
        if (isCorrect)
        {
            if (!isLastIngredient)
                DialogueManager.Instance.StartDialogue(11);

            closeBestiaryButton.OnPointerClick(new PointerEventData(EventSystem.current));

            IngridientModel.SetActive(true);
            isCorrect = false;
        }
        else
        {
            DialogueManager.Instance.StartDialogue(10);
        }
    }
}