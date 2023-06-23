using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class taimer : NetworkBehaviour
{
    [SerializeField] private TMP_Text ShowTime;
    [SerializeField] private int time;
    public NetworkVariable<int> NetworkTime = new(0);
    [SerializeField] private ShowRecepiesUI showRecepiesUI;
    private int reloadtime;

    private Coroutine serverClockCoroutine = null;
    private Coroutine clockCoroutine = null;

    public static taimer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        reloadtime = time;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            serverClockCoroutine = StartCoroutine(Clock());

        StartTimerServerRpc();
    }

    private IEnumerator Clock()
    {
        while (IsServer)
        {
            if (time <= 0)
            {
                ResetTimer();
            }

            time -= 1;
            NetworkTime.Value = time;

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
            time = NetworkTime.Value;

            if (time / 60 <= 10)
            {
                if (time % 60 < 10)
                    ShowTime.text = "0" + time / 60 + ":" + "0" + time % 60;
                else
                    ShowTime.text = "0" + time / 60 + ":" + time % 60;
            }
            else
            {
                if (time % 60 < 10)
                    ShowTime.text = time / 60 + ":" + "0" + time % 60;
                else
                    ShowTime.text = time / 60 + ":" + time % 60;
            }

            yield return new WaitForSeconds(1);
        }
    }

    // call on Server
    public void ResetTimer()
    {
        transform.parent.gameObject.SetActive(false);
        showRecepiesUI.Hide();
        time = reloadtime + 3;
    }
}
