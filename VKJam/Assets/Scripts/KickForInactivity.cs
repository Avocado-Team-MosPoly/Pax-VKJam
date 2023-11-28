using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KickForInactivity : NetworkBehaviour
{
    private void Start()
    {
        if (IsServer)
            StartCoroutine("Kick");
    }
    IEnumerator Kick()
    {
        yield return new WaitForSeconds(300);    
        RelayManager.Instance.Disconnect();
    }
}
