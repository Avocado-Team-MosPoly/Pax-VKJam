using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.Events;

public class Timer : NetworkBehaviour
{
    [SerializeField] private TMP_Text ShowTime;
    public NetworkVariable<int> NetworkTime = new(0);
    private bool isTimePaused = false;
    [SerializeField] private ShowRecepiesUI showRecepiesUI;
    [SerializeField] private int roundTime = 30;

    private Coroutine serverClockCoroutine = null;

    [HideInInspector] public UnityEvent OnExpired;

    public static Timer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        NetworkTime.OnValueChanged += OnTaimerChange;
        
        if (IsServer)
            NetworkTime.Value = roundTime;
    }

    /// <summary> Call only on server </summary>
    [ServerRpc]
    public void StartTimerServerRpc()
    {
        if (serverClockCoroutine == null)
            serverClockCoroutine = StartCoroutine(Clock());

        SetPause(false);
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
            }

            NetworkTime.Value -= 1;
            
            yield return new WaitForSeconds(1);
        }
    }

    // call on Server
    public void ResetToDefault()
    {

        transform.parent.gameObject.SetActive(false);
        showRecepiesUI.HideRecepi();

        NetworkTime.Value = roundTime;
        isTimePaused = true;

        TimerEndClientRpc();
    }

    private void OnTaimerChange(int preveusValue, int newValue)
    {
        
        if (NetworkTime.Value / 60 <= 10)
        {
            if (NetworkTime.Value % 60 < 10)
                ShowTime.text = "0" + NetworkTime.Value / 60 + ":" + "0" + NetworkTime.Value % 60;
            else
                ShowTime.text = "0" + NetworkTime.Value / 60 + ":" + NetworkTime.Value % 60;
        }
        else
        {
            if (NetworkTime.Value % 60 < 10)
                ShowTime.text = NetworkTime.Value / 60 + ":" + "0" + NetworkTime.Value % 60;
            else
                ShowTime.text = NetworkTime.Value / 60 + ":" + NetworkTime.Value % 60;
        }
    }

    [ClientRpc]
    public void TimerEndClientRpc()
    {

        showRecepiesUI.SetRecepi("����� ���������");

    }

    public void SetPause(bool state)
    {
        isTimePaused = state;
    }
}
