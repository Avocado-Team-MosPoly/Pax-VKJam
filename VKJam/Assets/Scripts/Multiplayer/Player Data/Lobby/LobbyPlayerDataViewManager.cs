using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerDataViewManager : MonoBehaviour
{
    [SerializeField] private LobbyManagerUI lobbyManagerUI;
    [SerializeField] private LobbyPlayerDataView[] playerDatas;

    [Header("Description")]
    [SerializeField] private TextMeshProUGUI textLabel;
    [SerializeField] private string kickPlayerText;

    [SerializeField] private Vector3 positionOffset = new(15f, 0f, 0f);

    [Header("Kick player confirmation")]
    [SerializeField] private GameObject kickPlayerConfirmationObject;
    [SerializeField] private Button kickPlayerConfirmButton;
    [SerializeField] private Button kickPlayerCancelButton;

    private LobbyPlayerDataView playerToKick;
    private RectTransform rectTransform;
    private bool canKick = true;

    private IReadOnlyDictionary<ulong, PlayerData> playerData => PlayersDataManager.Instance.PlayerDatas;
    private StoreSection avatarsAndFramesStorage => PlayersDataManager.Instance.AvatarsAndFramesStorage;

    private void Awake()
    {
        rectTransform = transform as RectTransform;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = Vector2.zero;

        foreach (LobbyPlayerDataView playerData in playerDatas)
            playerData.gameObject.SetActive(false);
    }

    private void Start()
    {
        foreach (LobbyPlayerDataView playerData in playerDatas)
        {
            playerData.PointerEntered.AddListener(Show);
            playerData.PointerExit.AddListener((LobbyPlayerDataView playerData) => Hide());
            playerData.Clicked.AddListener(KickPlayer);
        }

        kickPlayerConfirmButton.onClick.AddListener(ConfirmKickPlayer);
        kickPlayerCancelButton.onClick.AddListener(CancelKickPlayerConfirmation);
        kickPlayerConfirmationObject.SetActive(false);

        gameObject.SetActive(false);

        foreach (byte playerId in lobbyManagerUI.PlayersId)
        {
            if (playerId != NetworkManager.Singleton.LocalClientId)
                AddPlayer(playerId);
        }

        foreach (ulong playerId in PlayersDataManager.Instance.PlayerDatas.Keys)
        {
            if (playerId != NetworkManager.Singleton.LocalClientId)
                AddPlayer(playerId);
        }
    }

    private void Update()
    {
        MoveToMousePosition();
    }

    private void MoveToMousePosition()
    {
        Vector3 newPosition = Input.mousePosition + positionOffset;

        if (newPosition.x + rectTransform.sizeDelta.x > Screen.width)
            newPosition.x = Input.mousePosition.x - positionOffset.x - rectTransform.sizeDelta.x;
        if (newPosition.y + rectTransform.sizeDelta.y > Screen.height)
            newPosition.y = Input.mousePosition.y - positionOffset.y - rectTransform.sizeDelta.y;

        transform.position = newPosition;
    }

    private void CancelKickPlayerConfirmation()
    {
        playerToKick = null;
        kickPlayerConfirmationObject.SetActive(false);
    }

    private void ConfirmKickPlayer()
    {
        if (playerToKick == null)
        {
            Logger.Instance.LogError(this, $"{nameof(playerToKick)} is null");
            return;
        }

        RelayManager.Instance.DisconnectPlayer(playerToKick.ClientId);
        LobbyManager.Instance.DisconnectPlayerAsync(playerToKick.ClientId);

        playerToKick = null;
        kickPlayerConfirmationObject.SetActive(false);
    }

    private void KickPlayer(LobbyPlayerDataView playerData)
    {
        if (!canKick || !NetworkManager.Singleton.IsServer || NetworkManager.Singleton.LocalClientId == playerData.ClientId)
            return;

        playerToKick = playerData;
        kickPlayerConfirmationObject.SetActive(true);
    }

    private void Show(LobbyPlayerDataView playerData)
    {
        textLabel.text = playerData.ClientName;

        if (canKick && NetworkManager.Singleton.IsServer && NetworkManager.Singleton.LocalClientId != playerData.ClientId)
            textLabel.text += "\n------\n" + kickPlayerText;

        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private LobbyPlayerDataView GetFirstInactivePlayerDataView()
    {
        foreach (LobbyPlayerDataView playerDataView in playerDatas)
        {
            if (!playerDataView.gameObject.activeSelf)
                return playerDataView;
        }

        return null;
    }

    private LobbyPlayerDataView GetPlayerDataViewById(ulong clientId)
    {
        foreach (LobbyPlayerDataView playerDataView in playerDatas)
        {
            if (playerDataView.ClientId == clientId)
                return playerDataView;
        }

        return null;
    }

    public void AddPlayer(ulong clientId)
    {
        if (playerDatas.Length < 0)
        {
            Logger.Instance.LogError(this, new System.ArgumentOutOfRangeException("Client Id shouldn't be less than 0"));
            return;
        }

        int playerDataViewIndex = lobbyManagerUI.PlayersId.IndexOf((byte)clientId);

        if (playerDataViewIndex < 0)
        {
            Logger.Instance.LogError(this, $"{nameof(playerDataViewIndex)} is not correct. Value = '{playerDataViewIndex}'");
            return;
        }
        else if (playerDatas[playerDataViewIndex].gameObject.activeSelf)
            return;

        LobbyPlayerDataView playerDataView = playerDatas[playerDataViewIndex];

        if (playerDataView == null)
        {
            Logger.Instance.LogError(this, $"{nameof(playerDataView)} is null. Index in {nameof(playerDatas)} is {playerDataViewIndex}");
            return;
        }

        if (playerDataView.gameObject.activeSelf)
            return;

        playerDataView.gameObject.SetActive(true);

        if (PlayersDataManager.Instance.PlayerDatas.ContainsKey(clientId))
        {
            playerDataView.SetData(PlayersDataManager.Instance.PlayerDatas[clientId].Name, clientId);

            try
            {
                //Logger.Instance.LogError(this, clientId + " " + playerData[clientId].AvatarIndex + " " + avatarsAndFramesStorage.products[playerData[clientId].AvatarIndex].Data.productName);
                //Logger.Instance.LogError(this, clientId + " " + playerData[clientId].AvatarFrameIndex + " " + avatarsAndFramesStorage.products[playerData[clientId].AvatarFrameIndex].Data.productName);
                Sprite avatar = avatarsAndFramesStorage.products[playerData[clientId].AvatarIndex].icon;
                Sprite frame = avatarsAndFramesStorage.products[playerData[clientId].AvatarFrameIndex].icon;

                playerDataView.SetAvatar(avatar);
                playerDataView.SetFrame(frame);
            }
            catch (System.NullReferenceException ex)
            {
                Logger.Instance.LogError(this, ex);
            }
        }
        else
        {
            playerDataView.SetData(null, clientId);
        }
    }

    public void RemovePlayer(ulong clientId)
    {
        if (playerDatas.Length < 0)
        {
            Logger.Instance.LogError(this, new System.ArgumentOutOfRangeException("Client Id shouldn't be less than 0"));
            return;
        }

        LobbyPlayerDataView playerDataView = GetPlayerDataViewById(clientId);
        if (playerDataView == null)
        {
            Logger.Instance.LogError(this, $"{nameof(playerDataView)} is null");
            return;
        }

        playerDataView.gameObject.SetActive(false);
    }

    public void LockKick() => canKick = false;
    public void UnlockKick() => canKick = false;

}