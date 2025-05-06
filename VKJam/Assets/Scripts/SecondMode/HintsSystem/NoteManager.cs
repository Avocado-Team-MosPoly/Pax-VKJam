using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class NoteManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField inputField; 
    [SerializeField] private Interactable[] noteObjects; 
    [SerializeField] private GameManager gameManager;

    private List<string> notes = new List<string>(5);
    private int currentIngredientIndex = 0;
    private void Start()
    {
        foreach (var note in noteObjects)
        {
            note.gameObject.SetActive(false);
            note.ActivityInteractable = false;
        }

        for (int i = 0; i < 5; i++)
        {
            notes.Add("");
        }

        inputField.onSubmit.AddListener(SaveNote);

        if (gameManager != null)
        {
            gameManager.OnIngredientSwitchedOnClient.AddListener(OnIngredientSwitched);
        }
    }


    private void SaveNote(string text)
    {
        currentIngredientIndex = gameManager.IngredientManager.GetCurrentIngredientIndex;

        if (currentIngredientIndex >= noteObjects.Length)
            return;

        notes[currentIngredientIndex] = text;

        noteObjects[currentIngredientIndex].gameObject.SetActive(true);

        TMP_Text noteText = noteObjects[currentIngredientIndex].GetComponentInChildren<TMP_Text>();
        if (noteText != null)
        {
            noteText.text = text;
        }
        Interactable interactable = noteObjects[currentIngredientIndex];
        if (interactable != null)
        {
            interactable.m_OnClick.RemoveAllListeners();
            interactable.m_OnClick.AddListener(() => RewriteNote(currentIngredientIndex));
        }
    }

    private void OnIngredientSwitched(int newIngredientIndex)
    {
        noteObjects[currentIngredientIndex].ActivityInteractable = false;
        currentIngredientIndex = newIngredientIndex;
        noteObjects[currentIngredientIndex].ActivityInteractable = true;
        inputField.ActivateInputField();
    }

    public void RewriteNote(int noteIndex)
    {
        if (noteIndex == currentIngredientIndex)
        {
            inputField.text = notes[noteIndex];
            inputField.ActivateInputField();
        }
    }

    public List<string> GetAllNotes()
    {
        return new List<string>(notes);
    }
}