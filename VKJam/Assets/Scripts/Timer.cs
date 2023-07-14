using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class Timer : NetworkBehaviour
{
    [SerializeField] private TMP_Text ShowTime;
    public NetworkVariable<int> NetworkTime = new(0);
    private bool isTimePaused = false;
    [SerializeField] private ShowRecepiesUI showRecepiesUI;
    [SerializeField] private int roundTime = 30;

    private Coroutine serverClockCoroutine = null;

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

    [ServerRpc]
    public void StartTimerServerRpc()
    {
        if (serverClockCoroutine==null)
            serverClockCoroutine = StartCoroutine(Clock());
    }

    private IEnumerator Clock()
    {
        while (IsServer)
        {
            if (NetworkTime.Value <= 0)
                ResetTimer();

            if (isTimePaused == false)
                NetworkTime.Value -= 1;

            yield return new WaitForSeconds(1);
        }
    }

    // call on Server
    public void ResetTimer()
    {
        transform.parent.gameObject.SetActive(false);
        showRecepiesUI.HideRecepi();
        NetworkTime.Value = roundTime;
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
        showRecepiesUI.SetRecepi("нужно назначить");

    }

    public void SetPause(bool state)
    {
        isTimePaused = state;
    }
}
