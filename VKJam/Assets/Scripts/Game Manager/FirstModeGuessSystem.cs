using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class FirstModeGuessSystem : NetworkBehaviour, IGuessSystem
{
    public UnityEvent<string, ulong> OnIngredientGuess { get; private set; } = new();
    public UnityEvent<string, ulong> OnMonsterGuess { get; private set; } = new();

    [SerializeField] private string[] chooseNotificationText;
    [SerializeField] private BestiaryIngredients bestiaryIngredients;
    [SerializeField] private Bestiary bestiary;

    [SerializeField] private PlayersStatusManager playersStatusManager;

    [ClientRpc]
    private void SendNotificationClientRpc(int guessId, byte senderClientId)
    {
        if (NotificationSystem.Instance == null)
            throw new NullReferenceException("Add a Notification System Prefab to the Menu scene to avoid this exception");

        string chosenThing = GameManager.Instance.Stage == Stage.IngredientGuess ? bestiaryIngredients.IngredientList[guessId].Name : bestiary.Monsters[guessId].Id;
        NotificationSystem.Instance.SendLocal($"{chooseNotificationText[0]} {PlayersDataManager.Instance.PlayersData[senderClientId].Name} {chooseNotificationText[1]} {chosenThing}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendAnswerServerRpc(int guessId, ServerRpcParams serverRpcParams)
    {
        if (GameManager.Instance.Stage == Stage.Waiting || guessId < 0)
            return;

        byte byteClientId = (byte)serverRpcParams.Receive.SenderClientId;
        string guess = GameManager.Instance.Stage == Stage.IngredientGuess ? bestiaryIngredients.IngredientList[guessId].Name : bestiary.Monsters[guessId].Id;

        if (playersStatusManager == null)
            Logger.Instance.LogError(this, new NullReferenceException($"{nameof(playersStatusManager)} is null"));
        else
        {
            if (guess == playersStatusManager.GetPlayerStatus(byteClientId))
                return;

            playersStatusManager.SendStatus(guess, serverRpcParams.Receive.SenderClientId);
        }

        //SendNotificationClientRpc(guessId, byteClientId);

        if (GameManager.Instance.Stage == Stage.IngredientGuess)
            OnIngredientGuess?.Invoke(guess, serverRpcParams.Receive.SenderClientId);
        else
            OnMonsterGuess?.Invoke(guess, serverRpcParams.Receive.SenderClientId);
    }
}