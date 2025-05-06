using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BestiaryHints : MonoBehaviour
{
    [SerializeField] private NoteManager noteManager;
    [SerializeField] private GameObject[] notePanels = new GameObject[5]; // Массив панелей записок

    private void Start()
    {
        foreach (var panel in notePanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    public void UpdateNotesDisplay()
    {

        List<string> notes = noteManager.GetAllNotes();

        for (int i = 0; i < notes.Count && i < notePanels.Length; i++)
        {
            if (notePanels[i] == null) continue;

            TMP_Text noteText = notePanels[i].GetComponentInChildren<TMP_Text>();

            if (!string.IsNullOrEmpty(notes[i]))
            {
                notePanels[i].SetActive(true);

                if (noteText != null)
                    noteText.text = notes[i];

            }
            else
                notePanels[i].SetActive(false);
        }
    }
}