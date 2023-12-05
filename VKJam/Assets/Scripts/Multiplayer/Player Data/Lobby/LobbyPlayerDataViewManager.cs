using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerDataViewManager : MonoBehaviour
{
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

    private IReadOnlyDictionary<ulong, PlayerData> playerData => PlayersDataManager.Instance.PlayerDatas;
    private storeSection avatarsAndFramesStorage => PlayersDataManager.Instance.AvatarsAndFramesStorage;

    private void Awake()
    {
        rectTransform = transform as RectTransform;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = Vector2.zero;
    }

    private void Start()
    {
        foreach (LobbyPlayerDataView playerData in playerDatas)
        {
            playerData.PointerEntered.AddListener(Show);
            playerData.PointerExit.AddListener((LobbyPlayerDataView playerData) => Hide());
            playerData.ClickedOnServer.AddListener(KickPlayer);
        }

        kickPlayerConfirmButton.onClick.AddListener(ConfirmKickPlayer);
        kickPlayerCancelButton.onClick.AddListener(CancelKickPlayerConfirmation);
        kickPlayerConfirmationObject.SetActive(false);

        gameObject.SetActive(false);
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
            return;

        RelayManager.Instance.DisconnectPlayer(playerToKick.ClientId);
        LobbyManager.Instance.DisconnectPlayer(playerToKick.ClientId);

        playerToKick = null;
        kickPlayerConfirmationObject.SetActive(false);
    }

    private void KickPlayer(LobbyPlayerDataView playerData)
    {
        playerToKick = playerData;
        kickPlayerConfirmationObject.SetActive(true);
    }

    private void Show(LobbyPlayerDataView playerData)
    {
        textLabel.text = playerData.ClientName;

        if (CustomNetworkManager.Singleton.IsServer)
            if (CustomNetworkManager.Singleton.LocalClientId != playerData.ClientId)
                textLabel.text += "\n------\n" + kickPlayerText;

        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void AddPlayer(ulong clientId)
    {
        if (playerDatas.Length < 0)
        {
            Logger.Instance.LogError(this, new System.ArgumentOutOfRangeException("Client Id shouldn't be less than 0"));
            return;
        }
        if (playerDatas.Length <= (int)clientId)
        {
            Logger.Instance.LogError(this, new System.ArgumentOutOfRangeException("Not enough player data for client"));
            return;
        }

        if (PlayersDataManager.Instance.PlayerDatas.ContainsKey(clientId))
        {
            playerDatas[clientId].SetData(PlayersDataManager.Instance.PlayerDatas[clientId].Name, clientId);

            try
            {
                Sprite avatar = avatarsAndFramesStorage.products[playerData[clientId].AvatarIndex].Model.GetComponent<Image>().sprite;
                Sprite frame = avatarsAndFramesStorage.products[playerData[clientId].AvatarFrameIndex].Model.GetComponent<Image>().sprite;

                playerDatas[clientId].SetAvatar(avatar);
                playerDatas[clientId].SetFrame(frame);
            }
            catch (System.NullReferenceException ex)
            {
                Logger.Instance.LogError(this, ex);
            }
        }
        else
            playerDatas[clientId].SetData(null, clientId);
    }
}