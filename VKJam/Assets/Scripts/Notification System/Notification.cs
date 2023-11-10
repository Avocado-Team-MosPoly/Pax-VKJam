using System;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public event Action OnDisappeared;
    public bool IsActive { get; private set; }

    [SerializeField] private TextMeshProUGUI text;

    public void SetData(string text)
    {
        this.text.text = text;
    }

    public void Send()
    {
        gameObject.SetActive(true);

        IsActive = true;
    }

    public void OnDisappear()
    {
        gameObject.SetActive(false);

        IsActive = false;
        OnDisappeared?.Invoke();
    }
}