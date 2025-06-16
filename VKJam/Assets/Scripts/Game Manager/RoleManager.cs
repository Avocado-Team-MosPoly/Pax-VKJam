using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class RoleManager : NetworkBehaviour
{
    private NetworkVariable<byte> painterId = new(byte.MaxValue);
    private List<ulong> lastPainterIds = new();

    public UnityEvent OnPainterSetted { get; private set; } = new();
    public UnityEvent OnGuesserSetted { get; private set; } = new();

    public byte PainterId => painterId.Value;
    public bool IsPainter => PainterId == NetworkManager.Singleton.LocalClientId;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => IsSpawned && GameManager.Instance.SceneObjectsManager.IsInitialized);
        
        painterId.OnValueChanged += OnPainterChanged;

        if (IsServer)
            painterId.Value = (byte)NetworkManager.ConnectedClientsIds[0];
        else
            InvokeRoleEvent();
    }
    
    public void ChangeRoles()
    {
        foreach (byte clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!lastPainterIds.Contains(clientId))
            {
                painterId.Value = clientId;
                return;
            }
        }

        // runs if all played for painter
        ClearLastPaintersClientRpc();
        painterId.Value = (byte)NetworkManager.Singleton.ConnectedClientsIds[0];
    }

    [ClientRpc]
    private void ClearLastPaintersClientRpc()
    {
        lastPainterIds.Clear();
    }

    private void InvokeRoleEvent()
    {
        Debug.LogWarning("fefe");
        if (IsPainter)
            OnPainterSetted?.Invoke();
        else
            OnGuesserSetted?.Invoke();
    }

    private void OnPainterChanged(byte previousValue, byte newValue)
    {
        lastPainterIds.Add(newValue);
        InvokeRoleEvent();
    }
}