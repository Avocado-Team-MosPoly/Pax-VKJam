using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class DialogueManager : MonoBehaviour
{

    public List<Dialogue> DialogueList = new List<Dialogue>();

    public static DialogueManager Instance;
    private int ID;

    [SerializeField] private Animator DialogueAnimation;

    private Queue<string> sentences;
    [SerializeField] private TextMeshProUGUI dialogueText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        sentences = new Queue<string>();

        StartDialogue(0);
    }

    public void StartDialogue (int DialogueID)
    {
        Debug.Log("Dialogue" + DialogueList[DialogueID].name);

        if(DialogueID != 0)
        {
            DialogueAnimation.Play("Open");
        }

        ID = DialogueID;

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
            EndDialogue(ID);
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
            yield return new WaitForSeconds(.03f); ;
        }
    }

    void EndDialogue(int DialogueID)
    {
        DialogueAnimation.Play("Close");
        DialogueList[DialogueID].EndDialogue?.Invoke();
        Debug.Log("End");
    }


}
