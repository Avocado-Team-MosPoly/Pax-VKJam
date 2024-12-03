using System;
using Unity.Netcode;

public class SecondModeCompareSystem : NetworkBehaviour
{
    public event Action<string, ulong> OnIngredientGuess;
    public event Action<string, ulong> OnMonsterGuess;

    [ServerRpc (RequireOwnership = false)]
    public void CompareAnswerServerRpc(string guess, ServerRpcParams serverRpcParams)
    {
        if (SecondModeManager.Instance.Stage != SecondModeStage.IngredientGuess &&
            SecondModeManager.Instance.Stage != SecondModeStage.MonsterGuess ||
            string.IsNullOrEmpty(guess))
            return;

        if (SecondModeManager.Instance.Stage == SecondModeStage.IngredientGuess)
            OnIngredientGuess?.Invoke(guess, serverRpcParams.Receive.SenderClientId);
        else
            OnMonsterGuess?.Invoke(guess, serverRpcParams.Receive.SenderClientId);
    }
}