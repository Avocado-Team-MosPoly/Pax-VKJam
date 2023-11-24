using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialTimer : MonoBehaviour
{
    [SerializeField] private int _timer = 0;
    [SerializeField] private TMP_Text ShowTime;
    [SerializeField] private TMP_Text IngridientText;
    [SerializeField] private AnimateList _bookAnimator;
    [SerializeField] private TutorialPaint paint;

    [SerializeField] private string ingredientName;

    private Coroutine ingredientTimerCoroutine;

    public void StartIngridientTimer()
    {
        if (ingredientTimerCoroutine == null)
            ingredientTimerCoroutine = StartCoroutine(IngridientTimer(30));
    }

    IEnumerator IngridientTimer(int sec)
    {
        _timer = sec;

        for (int i = 0; i < sec && _timer > 0; i++)
        {
            _timer--;
            ShowTime.text = ToTimeFormat(_timer);

            yield return new WaitForSeconds(1f);
        }

        IngridientText.text = ingredientName;
        _timer = sec;
        paint.ClearCanvas();

        for (int i = 0; i < sec && _timer > 0; i++)
        {
            _timer--;
            ShowTime.text = ToTimeFormat(_timer);

            yield return new WaitForSeconds(1f);
        }

        paint.ClearCanvas();
        _bookAnimator.Play("NoteBookClose");
        DialogueManager.Instance.StartDialogue(6);

        ingredientTimerCoroutine = null;
    }

    private string ToTimeFormat(int seconds)
    {
        string timeString = (seconds / 60).ToString() + ":";
        int onlySeconds = seconds % 60;

        timeString += onlySeconds < 10 ? "0" + onlySeconds : onlySeconds;

        return timeString;
    }
}
