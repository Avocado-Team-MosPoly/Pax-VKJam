using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialIngridientChoose : MonoBehaviour
{
    [SerializeField] private bool CurrentIngridient;
    [SerializeField] private GameObject IngridientModel = null;

    private int _correctAnswers = 0;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(CheckIngridient);
    }
    public void SetIngridient(bool currentIngridient)
    {
        CurrentIngridient = currentIngridient;
    }

    public void CheckIngridient()
    {
        if(_correctAnswers != 3)
        {
            if (CurrentIngridient)
            {
                DialogueManager.Instance.StartDialogue(11);
                _correctAnswers++;
                IngridientModel.SetActive(true);
            }
            else
            {
                DialogueManager.Instance.StartDialogue(10);
            }
        }
        else
        {
            DialogueManager.Instance.StartDialogue(12);
        }
           

    }

}
