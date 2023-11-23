using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayersStatusManager : NetworkBehaviour
{
    [SerializeField] private BestiaryIngredients bestiaryIngredients;
    [SerializeField] private PlayerStatus playerStatusPrefab;

    [SerializeField] private string defaultPlayerStatus = "";
    [SerializeField] private Texture defaultPlayerProfileTexture;

    [SerializeField] private RectTransform playerStatusesContainer;
    [SerializeField] private PlayerStatusDescription playerStatusDescription;

    private Dictionary<ulong, PlayerStatus> playerStatuses = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            SetActive(false);

        StartCoroutine(OnNetworkSpawnCoroutine());
    }

    private IEnumerator OnNetworkSpawnCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (IsServer)
        {
            foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
                CreatePlayerStatusClientRpc((byte)clientId);
        }
    }

    [ClientRpc]
    private void CreatePlayerStatusClientRpc(byte ownerClientId)
    {
        PlayerStatus playerStatusInstance = Instantiate(playerStatusPrefab, playerStatusesContainer).
            Init(ownerClientId, /*URL_Image.ProfileTexture ?? */defaultPlayerProfileTexture, defaultPlayerStatus, playerStatusDescription);

        playerStatuses.Add(ownerClientId, playerStatusInstance);

        if (ownerClientId == NetworkManager.LocalClientId)
            playerStatusInstance.gameObject.SetActive(false);
    }

    private void UpdateStatus(string status, ulong senderClientId)
    {
        if (!playerStatuses.ContainsKey(senderClientId))
        {
            Debug.LogError($"[{nameof(PlayersStatusManager)}] Client {senderClientId} is not detected");
            return;
        }

        playerStatuses[senderClientId].SetStatus(status);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendStatusServerRpc(string status, byte senderClientId)
    {
        SendStatusClientRpc(status, senderClientId);
    }

    [ClientRpc]
    private void SendStatusClientRpc(string status, byte senderClientId)
    {
        UpdateStatus(status, senderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendStatusServerRpc(int id, byte senderClientId)
    {
        SendStatusClientRpc(id, senderClientId);
    }

    [ClientRpc]
    private void SendStatusClientRpc(int ingredientId, byte senderClientId)
    {
        UpdateStatus(bestiaryIngredients.IngredientList[ingredientId].Name, senderClientId);
    }

    public void SendStatus(string status, ulong senderClientId)
    {
        if (GameManager.Instance.Stage == Stage.IngredientGuess)
        {
            int ingredientIdIndex = bestiaryIngredients.GetIngredientIndexById(status);

            if (ingredientIdIndex < 0)
                throw new System.ArgumentOutOfRangeException($"[{nameof(PlayersStatusManager)}] Ingredient doesn't exist");

            if (IsServer)
                SendStatusClientRpc(ingredientIdIndex, (byte)senderClientId);
            else
                SendStatusServerRpc(ingredientIdIndex, (byte)senderClientId);
        }
        else
        {
            if (IsServer)
                SendStatusClientRpc(status, (byte)senderClientId);
            else
                SendStatusServerRpc(status, (byte)senderClientId);
        }
    }

    public void SetActive(bool value)
    {
        playerStatusesContainer.gameObject.SetActive(value);
    }

    public void ResetStatuses()
    {
        foreach (PlayerStatus playerStatus in playerStatuses.Values)
            playerStatus.ResetStatus(defaultPlayerStatus);
    }
}