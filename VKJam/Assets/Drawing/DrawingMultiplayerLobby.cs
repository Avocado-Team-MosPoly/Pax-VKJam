using Unity.Netcode;
using UnityEngine;

public class DrawingMultiplayerLobby : DrawingMultiplayer
{
    private NetworkVariable<byte> painterId = new(byte.MaxValue);
    private bool IsDrawing => painterId.Value == byte.MaxValue || painterId.Value == NetworkManager.LocalClientId;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isEnabled = true;
    }

    protected override void OnMousePositionValueChanged(Vector3 previousValue, Vector3 newValue)
    {
        if (IsDrawing)
            return;

        MoveBrush(newValue);
    }

    #region Start/Stop Drawing

    [ServerRpc(RequireOwnership = false)]
    protected override void StartDrawingServerRpc(Vector3 startPos, ServerRpcParams rpcParams)
    {
        painterId.Value = (byte)rpcParams.Receive.SenderClientId;
        base.StartDrawingServerRpc(startPos, rpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    protected override void StopDrawingServerRpc()
    {
        painterId.Value = byte.MaxValue;
        base.StopDrawingServerRpc();
    }

    #endregion
}