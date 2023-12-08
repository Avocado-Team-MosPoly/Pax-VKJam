using System;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public event Action OnDisappeared;
    public bool IsActive { get; private set; }

    [SerializeField] private TextMeshProUGUI tmProUGUI;

    private Animator animator;

    public Notification Init()
    {
        animator = GetComponent<Animator>();

        return this;
    }

    public void SetData(string text, float showTime)
    {
        if (IsActive)
        {
            Logger.Instance.LogWarning(this, "Can't change notification because it is active");
            return;
        }

        tmProUGUI.text = text;
        animator.speed = 1f / showTime;
    }

    public void Send()
    {
        IsActive = true;
        gameObject.SetActive(IsActive);
    }

    public void OnDisappear()
    {
        Logger.Instance.Log(this, $"Text: {tmProUGUI.text}"); // Show time: {1f / animator.speed});

        IsActive = false;
        gameObject.SetActive(IsActive);

        OnDisappeared?.Invoke();
    }
}