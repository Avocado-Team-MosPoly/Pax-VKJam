using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayersStatusManager : NetworkBehaviour
{
    [SerializeField] private BestiaryIngredients bestiaryIngredients;
    [SerializeField] private PlayerStatus playerStatusPrefab;

    [SerializeField] private string defaultPlayerStatus = string.Empty;

    [SerializeField] private RectTransform playerStatusesContainer;
    [SerializeField] private PlayerStatusDescription playerStatusDescription;

    private Dictionary<ulong, PlayerStatus> playerStatuses = new();

    private IReadOnlyDictionary<ulong, PlayerData> PlayerDatas => PlayersDataManager.Instance.PlayerDatas;
    private StoreSection AvatarsAndFramesStorage => PlayersDataManager.Instance.AvatarsAndFramesStorage;

    public override void OnNetworkSpawn()
    {
        StartCoroutine(OnNetworkSpawnCoroutine());
    }

    public void SendStatus(string status, ulong senderClientId)
    {
        if (status == GetPlayerStatus(senderClientId))
            return;

        if (GameManager.Instance.Stage == Stage.IngredientGuess && bestiaryIngredients != null)
        {
            int ingredientIdIndex = bestiaryIngredients.GetIngredientIndexById(status);

            if (ingredientIdIndex < 0)
            {
                Logger.Instance.LogError(this, "Ingredient doesn't exist");
                return;
            }

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
        //Logger.Instance.Log($"[{nameof(PlayersStatusManager)}] Set Active {value}");
    }

    public void ResetStatuses()
    {
        foreach (PlayerStatus playerStatus in playerStatuses.Values)
            playerStatus.ResetStatus(defaultPlayerStatus);
    }

    public string GetPlayerStatus(ulong clientId)
    {
        foreach (var playerStatus in playerStatuses)
            if (playerStatus.Key == clientId)
                return playerStatus.Value.GuessStatusText;

        return null;
    }

    private IEnumerator OnNetworkSpawnCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (IsServer)
        {
            foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
                CreatePlayerStatusClientRpc((byte)clientId);
        }

        GameManager.Instance.RoleManager.OnPainterSetted.AddListener(OnRolesChanged);
        GameManager.Instance.RoleManager.OnGuesserSetted.AddListener(OnRolesChanged);
    }

    [ClientRpc]
    private void CreatePlayerStatusClientRpc(byte ownerClientId)
    {
        Sprite avatar = AvatarsAndFramesStorage.products[PlayerDatas[ownerClientId].AvatarIndex].icon;
        Sprite frame = AvatarsAndFramesStorage.products[PlayerDatas[ownerClientId].AvatarFrameIndex].icon;

        PlayerStatus playerStatusInstance = Instantiate(playerStatusPrefab, playerStatusesContainer);
        playerStatusInstance.Init(ownerClientId, avatar, frame, defaultPlayerStatus, playerStatusDescription);

        playerStatuses.Add(ownerClientId, playerStatusInstance);

        playerStatusInstance.gameObject.SetActive(false);

        if (GameManager.Instance.IsTeamMode)
        {
            if (ownerClientId != NetworkManager.LocalClientId && ownerClientId != GameManager.Instance.PainterId)
                playerStatusInstance.gameObject.SetActive(true);
        }
        else
        {
            if (ownerClientId != NetworkManager.LocalClientId && GameManager.Instance.IsPainter)
                playerStatusInstance.gameObject.SetActive(true);
        }
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
    private void SendStatusServerRpc(int id, byte senderClientId)
    {
        SendStatusClientRpc(id, senderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendStatusServerRpc(string status, byte senderClientId)
    {
        SendStatusClientRpc(status, senderClientId);
    }

    [ClientRpc]
    private void SendStatusClientRpc(int ingredientId, byte senderClientId)
    {
        UpdateStatus(bestiaryIngredients.IngredientList[ingredientId].Name, senderClientId);
    }

    [ClientRpc]
    private void SendStatusClientRpc(string status, byte senderClientId)
    {
        UpdateStatus(status, senderClientId);
    }

    private void OnRolesChanged()
    {
        foreach (PlayerStatus playerStatus in playerStatuses.Values)
        {
            if (GameManager.Instance.IsTeamMode)
            {
                if (playerStatus.OwnerClientId != NetworkManager.LocalClientId && playerStatus.OwnerClientId != GameManager.Instance.PainterId)
                    playerStatus.gameObject.SetActive(true);
                else
                    playerStatus.gameObject.SetActive(false);
            }
            else
            {
                if (playerStatus.OwnerClientId != NetworkManager.LocalClientId && GameManager.Instance.IsPainter)
                    playerStatus.gameObject.SetActive(true);
                else
                    playerStatus.gameObject.SetActive(false);
            }
        }
    }
}