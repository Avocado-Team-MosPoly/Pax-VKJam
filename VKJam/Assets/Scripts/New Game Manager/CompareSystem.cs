using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class CompareSystem : NetworkBehaviour
{
    [HideInInspector] public UnityEvent<string, ulong> OnIngredientGuess;
    [HideInInspector] public UnityEvent<string, ulong> OnMonsterGuess;


    [ServerRpc(RequireOwnership = false)]
    public void CompareAnswerServerRpc(string guess, ServerRpcParams serverRpcParams)
    {
        if (GameManager.Instance.Stage == Stage.Waiting)
            return;

        if (GameManager.Instance.Stage == Stage.IngredientGuess)
            OnIngredientGuess?.Invoke(guess, serverRpcParams.Receive.SenderClientId);
        else
            OnMonsterGuess?.Invoke(guess, serverRpcParams.Receive.SenderClientId);
    }
}