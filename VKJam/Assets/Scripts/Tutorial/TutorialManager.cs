using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject Cards;
    [SerializeField] private TMP_Text TokenText;
    [SerializeField] private Animator _cameraAnimator;

    private int _firstSelectedCard = 0;
    private bool _check = true;

    public void EndDialogue_1()
    {
        StartCoroutine(NextDialogueStage(1,1));
    }

    public void EndDialogue_2()
    {
        StartCoroutine(NextDialogueStage(0.7f,2));
    }

    public void EndDialogue_3()
    {
        StartCoroutine(NextDialogueStage(1, 3));
    }

    public void EndDialogue_4()
    {
        StartCoroutine(NextDialogueStage(1, 4));
    }

    public void EndDialogue_5()
    {
        StartCoroutine(NextDialogueStage(0.5f, 5));
    }

    public void EndDialogue_7()
    {
        StartCoroutine(NextDialogueStage(1.5f, 7));
        TokenText.text = "X6";   
    }

    public void EndDialogue_8()
    {
        StartCoroutine(NextDialogueStage(1f, 8));
    }

    public void EndDialogue_9()
    {
        if(_check)
            StartCoroutine(NextDialogueStage(1f, 9));
        _check = false;
    }

    public void EndDialogue_15()
    {
        StartCoroutine(NextDialogueStage(1f, 15));
        TokenText.text = "X13";
    }

    IEnumerator NextDialogueStage(float Seconds, int DialogueID)
    {
        yield return new WaitForSeconds(Seconds);
        DialogueManager.Instance.StartDialogue(DialogueID);
    }


    public void CardChoose()
    {
        if (_firstSelectedCard == 0)
        {
            _firstSelectedCard++;
        }
        else
        {
            _cameraAnimator.Play("CameraAnimBack");
            Cards.SetActive(false);
            EndDialogue_3();
        }
    }

}
