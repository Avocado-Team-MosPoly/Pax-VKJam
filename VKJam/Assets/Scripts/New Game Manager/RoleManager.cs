using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class RoleManager : NetworkBehaviour
{
    private NetworkVariable<byte> painterId = new(byte.MaxValue);
    private List<ulong> lastPainterIds = new();

    [HideInInspector] public UnityEvent OnPainterSetted;
    [HideInInspector] public UnityEvent OnGuesserSetted;
    public byte PainterId => painterId.Value;

    public override void OnNetworkSpawn()
    {
        painterId.OnValueChanged += PainterId_OnValueChanged;

        if (IsServer)
        {
            painterId.Value = (byte)NetworkManager.ConnectedClientsIds[0];
        }
        else
        {
            ChooseRole();
        }
    }

    private void PainterId_OnValueChanged(byte previousValue, byte newValue)
    {
        Log("PainterId_OnValueChanged");

        lastPainterIds.Add(newValue);
        ChooseRole();
    }

    private void ChooseRole()
    {
        if (GameManager.Instance.IsPainter)
            OnPainterSetted?.Invoke();
        else
            OnGuesserSetted?.Invoke();
    }

    [ClientRpc]
    private void ClearLastPaintersClientRpc()
    {
        lastPainterIds.Clear();
    }

    private void Log(object message) => Debug.Log($"{name} {message}");

    public void ChangeRoles()
    {
#if UNITY_EDITOR
        // forgot why
        GameManager.Instance.CardManager.enabled = false;
        OnPainterSetted?.Invoke();
#endif

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
}