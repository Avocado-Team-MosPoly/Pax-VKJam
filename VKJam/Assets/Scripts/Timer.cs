using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.Events;
using Unity.Collections.LowLevel.Unsafe;

public class Timer : NetworkBehaviour
{
    [SerializeField] private TMP_Text ShowTime;
    public NetworkVariable<int> NetworkTime = new(0);
    private bool isTimePaused = false;
    [SerializeField] private Hint showRecepiesUI;
    [SerializeField] private int roundTime = 30;

    private Coroutine serverClockCoroutine = null;

    [HideInInspector] public UnityEvent OnExpired;

    public static Timer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        ShowTime.text = ToTimeFormat(30);
    }

    public override void OnNetworkSpawn()
    {
        NetworkTime.OnValueChanged += OnTaimerChange;
        
        if (IsServer)
            NetworkTime.Value = roundTime;
    }

    [ServerRpc]
    public void StartServerRpc()
    {
        ResetToDefault();
        
        if (serverClockCoroutine == null)
            serverClockCoroutine = StartCoroutine(Clock());

        SetPause(false);
    }

    [ServerRpc]
    public void StopServerRpc()
    {
        StopCoroutine(serverClockCoroutine);
        serverClockCoroutine = null;

        ResetToDefault();
    }

    private IEnumerator Clock()
    {
        while (IsServer)
        {
            if (isTimePaused)
            {
                yield return new WaitForSeconds(1);
                continue;
            }

            if (NetworkTime.Value <= 0)
            {
                //ResetTimer();
                OnExpired?.Invoke();
                StopServerRpc();
            }
            else
                NetworkTime.Value -= 1;
            
            yield return new WaitForSeconds(1);
        }
    }

    public void ResetToDefault()
    {
        NetworkTime.Value = roundTime;
    }

    private void OnTaimerChange(int previousValue, int newValue)
    {
        if (newValue < 0)
            return;

        ShowTime.text = ToTimeFormat(newValue);
    }

    private string ToTimeFormat(int seconds)
    {
        string timeString = string.Empty;

        if (seconds / 60 <= 10)
        {
            if (seconds % 60 < 10)
                timeString = "0" + seconds / 60 + ":" + "0" + seconds % 60;
            else
                timeString = "0" + seconds / 60 + ":" + seconds % 60;
        }
        else
        {
            if (seconds % 60 < 10)
                timeString = seconds / 60 + ":" + "0" + seconds % 60;
            else
                timeString = seconds / 60 + ":" + seconds % 60;
        }

        return timeString;
    }

    public void SetPause(bool state)
    {
        isTimePaused = state;
    }
}
