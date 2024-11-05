using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkCountdownTimer : NetworkBehaviour
{
    public bool IsPlaying { get; private set; }
    public float RemainingTime => remainingTime.Value;

    public event Action<float> ValueChanged;
    public event Action Finished;

    private NetworkVariable<float> remainingTime;

    public override void OnNetworkSpawn()
    {
        remainingTime = new NetworkVariable<float>(0);
        remainingTime.OnValueChanged += OnValueChanged;
    }

    private void Update()
    {
        if (!IsPlaying)
            return;

        remainingTime.Value -= Time.deltaTime;
    }

    public void Play(float time)
    {
        if (!IsServer)
            return;

        remainingTime.Value = time;
        IsPlaying = true;
    }

    public void Stop()
    {
        if (!IsServer)
            return;

        IsPlaying = false;
    }

    public void Continue()
    {
        if (!IsServer)
            return;

        IsPlaying = true;
    }

    public void ClearFinishedEvent()
    {
        Finished = null;
    }

    private void OnValueChanged(float prevTime, float newTime)
    {
        ValueChanged?.Invoke(newTime);

        if (newTime <= 0f)
            OnFinished();
    }

    private void OnFinished()
    {
        if (IsServer) // not requred, but more clear
        {
            IsPlaying = false;
        }

        Finished?.Invoke();
    }
}