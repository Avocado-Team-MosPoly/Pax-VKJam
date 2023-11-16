using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEditor.PackageManager;
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

    private void Awake()
    {
        playerStatusesContainer.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(OnNetworkSpawnCoroutine());
    }

    private IEnumerator OnNetworkSpawnCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (IsServer)
        {
            foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
            {
                CreatePlayerStatusClientRpc((byte)clientId);
            }

            playerStatusesContainer.gameObject.SetActive(true);
        }
    }

    [ClientRpc]
    private void CreatePlayerStatusClientRpc(byte clientId)
    {
        PlayerStatus playerStatusInstance = Instantiate(playerStatusPrefab, playerStatusesContainer).
            Init(clientId, URL_Image.ProfileTexture ?? defaultPlayerProfileTexture, defaultPlayerStatus, playerStatusDescription);

        playerStatuses.Add(clientId, playerStatusInstance);
    }

    private void UpdateStatus(string status, ulong senderClientId)
    {
        if (!playerStatuses.ContainsKey(senderClientId))
        {
            Debug.LogError($"[{this.name}] Client {senderClientId} is not detected");
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
                throw new System.ArgumentOutOfRangeException("Ingredient doesn't exist");

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

    public void OnRoundEnded()
    {
        foreach (PlayerStatus playerStatus in playerStatuses.Values)
            playerStatus.ResetStatus(defaultPlayerStatus);
    }
}