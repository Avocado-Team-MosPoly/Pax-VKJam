using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class taimer : NetworkBehaviour
{
    [SerializeField] private TMP_Text ShowTime;
    [SerializeField] private int time;
    private NetworkVariable<int> networkTime = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private ShowRecepiesUI showRecepiesUI;
    private int reloadtime;

    private Coroutine clockCoroutine = null;

    private void Start()
    {
        reloadtime = time;
    }

    private void OnEnable()
    {
        if (IsServer)
        {
            clockCoroutine = StartCoroutine(Clock());
            NetworkManager.Singleton.OnClientConnectedCallback += (ulong id) => { StartTimerClientRpc(); };
        }
        else
        {
            Debug.LogWarning("Not Host");
        }
    }

    IEnumerator Clock()
    {
        while (IsHost)
        {
            if (time <= 0)
            {
                transform.parent.gameObject.SetActive(false);
                showRecepiesUI.Hide();
                time = reloadtime + 3;
            }

            time -= 1;
            if (time / 60 <= 10)
            {
                ShowTime.text = "0" + time / 60 + ":" + time % 60;
                if (time % 60 < 10)
                {
                    ShowTime.text = "0" + time / 60 + ":" + "0" + time % 60;
                }
            }
            else
            {
                ShowTime.text = time / 60 + ":" + time % 60;
                if (time % 60 < 10)
                {
                    ShowTime.text = time / 60 + ":" + "0" + time % 60;
                    
                    networkTime.Value = time;
                }
            }
            yield return new WaitForSeconds(1);
        }

        clockCoroutine = null;
    }

    [ClientRpc]
    public void StartTimerClientRpc()
    {
        if (!IsHost)
        {
            if (clockCoroutine == null)
                clockCoroutine = StartCoroutine(ClientClock());
        }
    }

    private IEnumerator ClientClock()
    {
        while (IsClient)
        {
            time = networkTime.Value;
            ShowTime.text = time / 60 + ":" + "0" + time % 60;

            yield return new WaitForSeconds(1);
        }

        clockCoroutine = null;
    }
}
