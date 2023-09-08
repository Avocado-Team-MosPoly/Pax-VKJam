using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class RoleManager : NetworkBehaviour
{
    private NetworkVariable<ushort> painterId = new(ushort.MaxValue);
    private List<ulong> lastPainterIds = new();

    [HideInInspector] public UnityEvent OnPainterSetted;
    [HideInInspector] public UnityEvent OnGuesserSetted;
    
    public ushort PainterId => painterId.Value;

    public override void OnNetworkSpawn()
    {
        painterId.OnValueChanged += PainterId_OnValueChanged;

        if (IsServer)
        {
            painterId.Value = (ushort)NetworkManager.ConnectedClientsIds[0];
        }
        else
        {
            StartCoroutine(ChooseRole());
        }
    }

    private void PainterId_OnValueChanged(ushort previousValue, ushort newValue)
    {
        Log("PainterId_OnValueChanged");

        lastPainterIds.Add(newValue);
        StartCoroutine(ChooseRole());
    }

    // не работает [ NetworkManager.Singleton.LocalClientId ], с IEnumerator почему-то работает
    private IEnumerator ChooseRole()
    {
        yield return null;

        if (GameManager.Instance.IsPainter)
            OnPainterSetted?.Invoke();
        else
            OnGuesserSetted?.Invoke();
    }

    private void ChangeRoles()
    {
        foreach (ushort clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!lastPainterIds.Contains(clientId))
            {
                painterId.Value = clientId;
                return;
            }
        }

        // runs if all played for painter
        ClearLastPaintersClientRpc();
        painterId.Value = (ushort)NetworkManager.Singleton.ConnectedClientsIds[0];
    }

    [ClientRpc]
    private void ClearLastPaintersClientRpc()
    {
        lastPainterIds.Clear();
    }

    private void Log(object message) => Debug.Log($"{name} {message}");
}