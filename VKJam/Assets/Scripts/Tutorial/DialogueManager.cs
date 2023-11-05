using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    public List<Dialogue> DialogueList = new List<Dialogue>();

    private Queue<string> sentences;
    [SerializeField] private TextMeshProUGUI dialogueText;

    private void Start()
    {
        sentences = new Queue<string>();

        StartDialogue(0);
    }

    public void StartDialogue (int DialogueID)
    {
        Debug.Log("Dialogue" + DialogueList[DialogueID].name);

        sentences.Clear();

        foreach (string sentence in DialogueList[DialogueID].sentences) 
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence (string sentence)
    {
        dialogueText.text = "";
        foreach(char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    void EndDialogue()
    {
        Debug.Log("End");
    }

}
