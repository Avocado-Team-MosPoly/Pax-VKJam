using Unity.Netcode;
using UnityEngine.Events;

public class SecondModeGuessSystem : NetworkBehaviour, IGuessSystem
{
    public UnityEvent<string, ulong> OnIngredientGuess { get; private set; } = new();
    public UnityEvent<string, ulong> OnMonsterGuess { get; private set; } = new();

    [ServerRpc (RequireOwnership = false)]
    public void SendAnswerServerRpc(string guess, ServerRpcParams serverRpcParams)
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