using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class taimer : NetworkBehaviour
{
    [SerializeField] private TMP_Text ShowTime;

    [SerializeField] private int roundTime = 30;
    public NetworkVariable<int> NetworkTime = new(0);
    [SerializeField] private ShowRecepiesUI showRecepiesUI;

    private Coroutine serverClockCoroutine = null;
    private Coroutine clockCoroutine = null;

    public static taimer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            serverClockCoroutine ??= StartCoroutine(Clock());

        StartTimerServerRpc();
    }

    private IEnumerator Clock()
    {
        while (IsServer)
        {
            if (NetworkTime.Value <= 0)
            {
                ResetTimer();
            }

            NetworkTime.Value -= 1;

            yield return new WaitForSeconds(1);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartTimerServerRpc()
    {
        StartTimerClientRpc();
    }

    [ClientRpc]
    public void StartTimerClientRpc()
    {
        clockCoroutine ??= StartCoroutine(ClientClock());
    }

    private IEnumerator ClientClock()
    {
        while (IsClient)
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

            yield return new WaitForSeconds(1);
        }
    }

    // call on Server
    public void ResetTimer()
    {
        transform.parent.gameObject.SetActive(false);
        showRecepiesUI.Hide();
        NetworkTime.Value = roundTime;
    }
}
