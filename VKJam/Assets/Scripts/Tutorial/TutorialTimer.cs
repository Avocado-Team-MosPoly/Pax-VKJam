using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialTimer : MonoBehaviour
{
    private int _timer = 0;
    [SerializeField] private TMP_Text ShowTime;
    [SerializeField] private TMP_Text IngridientText;
    [SerializeField] private AnimateList _bookAnimator;
    public void StartIngridientTimer()
    {
        _timer = 30;
        StartCoroutine(IngridientTimer(30));
    }


    IEnumerator IngridientTimer(int sec)
    {
        for (int i = 0; i < sec; i++)
        {
            ShowTime.text = ToTimeFormat(_timer);
            _timer--;
            yield return new WaitForSeconds(1);
        }

        IngridientText.text = "—вет€ща€с€ €года";
        _timer = 30;

        for (int i = 0; i < sec; i++)
        {
            ShowTime.text = ToTimeFormat(_timer);
            _timer--;
            yield return new WaitForSeconds(1);
        }

        _bookAnimator.Play("NoteBookClose");
        DialogueManager.Instance.StartDialogue(6);

    }

    private string ToTimeFormat(int seconds)
    {
        string timeString = (seconds / 60).ToString() + ":";
        int onlySeconds = seconds % 60;

        timeString += onlySeconds < 10 ? "0" + onlySeconds : onlySeconds;

        return timeString;
    }
}
