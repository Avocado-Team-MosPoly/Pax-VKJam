using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject CardPrefab;

    public void EndDialogue_1()
    {
        StartCoroutine(NextDialogueStage(1,1));
    }

    public void EndDialogue_2()
    {
        SetCardPrefabInteractable(false);
        StartCoroutine(NextDialogueStage(1,2));
    }

    IEnumerator NextDialogueStage(float Seconds, int DialogueID)
    {
        yield return new WaitForSeconds(Seconds);
        DialogueManager.Instance.StartDialogue(DialogueID);
    }

    public void SetCardPrefabInteractable(bool check)
    {
        CardPrefab.GetComponent<Interactable>().SetInteractable(check);
    }
}
