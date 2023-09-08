using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class CompareSystem : MonoBehaviour
{
    [HideInInspector] public UnityEvent<string, ulong> OnGuess;
    
    [ServerRpc(RequireOwnership = false)]
    public void CompareAnswerServerRpc(string guess, ServerRpcParams serverRpcParams)
    {
        if (GameManager.Instance.Stage == Stage.Waiting)
            return;

        OnGuess?.Invoke(guess, serverRpcParams.Receive.SenderClientId);
    }
}