using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public bool IsStopped { get; private set; } = true;
    public bool IsExpired { get; private set; } = true;

    public event Action<float> TimeChanged;

    private event Action expired;
    private float time;
    private bool destroyOnExpired;

    private void Update()
    {
        if (IsStopped)
            return;

        time = Mathf.Max(time - Time.deltaTime, 0);
        TimeChanged?.Invoke(time);

        if (time <= 0)
            OnExpired();
    }

    private void OnExpired()
    {
        IsExpired = true;
        IsStopped = true;
        expired?.Invoke();
        expired = null;

        if (destroyOnExpired)
            Destroy(this);
    }

    public void StartTimer(float time, Action expired, bool destroyOnExpired = false)
    {
        this.expired = expired;
        this.destroyOnExpired = destroyOnExpired;

        if (time <= 0f)
        {
            OnExpired();
            return;
        }

        this.time = time;
        IsStopped = false;
        IsExpired = false;
    }

    public void Continue()
    {
        IsStopped = false;
    }

    public void Stop()
    {
        IsStopped = true;
    }

    public void Destroy()
    {
        Stop();
        Destroy(this);
    }
}