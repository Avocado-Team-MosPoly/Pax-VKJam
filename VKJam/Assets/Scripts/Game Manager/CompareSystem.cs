using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class CompareSystem : NetworkBehaviour
{
    [HideInInspector] public UnityEvent<string, ulong> OnIngredientGuess;
    [HideInInspector] public UnityEvent<string, ulong> OnMonsterGuess;

    [SerializeField] private string[] chooseNotificationText;
    [SerializeField] private BestiaryIngredients bestiaryIngredients;
    [SerializeField] private Bestiary bestiary;

    [SerializeField] private PlayersStatusManager playersStatusManager;

    [ClientRpc]
    private void SendNotificationClientRpc(int guessId, byte senderClientId)
    {
        if (NotificationSystem.Instance == null)
            throw new System.NullReferenceException("Add a Notification System Prefab to the Menu scene to avoid this exception");

        string choosedThing = GameManager.Instance.Stage == Stage.IngredientGuess ? bestiaryIngredients.IngredientList[guessId].Name : bestiary.Monsters[guessId].id;
        string message = $"{chooseNotificationText[0]} {senderClientId} {chooseNotificationText[1]} {choosedThing}";
        // TODO: Show player name instead of id
        NotificationSystem.Instance.SendLocal(message);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CompareAnswerServerRpc(int guessId, ServerRpcParams serverRpcParams)
    {
        if (GameManager.Instance.Stage == Stage.Waiting)
            return;

        if (guessId < 0)
            return;

        byte byteClientId = (byte)serverRpcParams.Receive.SenderClientId;


        string guess = GameManager.Instance.Stage == Stage.IngredientGuess ? bestiaryIngredients.IngredientList[guessId].Name : bestiary.Monsters[guessId].id;

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