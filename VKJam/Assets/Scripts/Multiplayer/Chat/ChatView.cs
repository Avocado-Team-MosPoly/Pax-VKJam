using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Chat))]
public class ChatView : MonoBehaviour
{
    [Header("Message Container")]
    [SerializeField] private RectTransform messageContainer;
    [SerializeField, Tooltip("Is not recommended set more than 50")] private int maxTogetherShownMessages = 20;

    [Header("Chat States")]
    [SerializeField] private GameObject activeChatObject;
    [SerializeField] private GameObject inactiveChatObject;

    [Header("Unread Messages")]
    [SerializeField] private GameObject unreadMessagesCountObject;
    [SerializeField] private TextMeshProUGUI unreadMessagesCountText;

    [Header("Prefabs")]
    [SerializeField] private GameObject messagePrefab;

    [Header("Roles")]
    [SerializeField] private GameObject painterText;

    private List<TextMeshProUGUI> messageTexts = new();

    private Chat chat;

    private int lastShownMessageIndex = -1;
    private bool isOpen;
    private int unreadMessagesCount;

    private bool IsChatFull() => messageTexts.Count >= maxTogetherShownMessages;
    private string GetPlayerNameById(byte playerId) => PlayersDataManager.Instance.PlayerDatas[playerId].Name;
    private string MessageToText(Chat.Message message) => GetPlayerNameById(message.senderId) + ": " + message.text.ToString();

    private void Start()
    {
        chat = GetComponent<Chat>();

        chat.OnMessageSended.AddListener(UpdateView);
        chat.OnMessageReceived.AddListener(OnMessageReceived);

        //foreach (Player player in LobbyManager.Instance.CurrentLobby.Players)
        //{
        //    if (player.Id == LobbyManager.Instance.PlayerId)
        //        Debug.Log("player " + player.Data["Player Name"].Value + " is me, id: " + player.Id);
        //}
    }

    private void SpawnMessage(Chat.Message message)
    {
        if (IsChatFull())
            return;

        GameObject messageInstance = Instantiate(messagePrefab, messageContainer);
        TextMeshProUGUI messageText = messageInstance.GetComponentInChildren<TextMeshProUGUI>();

        if (messageText == null)
            throw new System.Exception("[Chat] Incorrect message prefab (Prefab should contain TextMeshProUGUI component)");

        messageText.text = MessageToText(message);
        messageTexts.Add(messageText);
    }

    private void UpdateShownMessages()
    {
        IReadOnlyList<Chat.Message> chatHistory = chat.History;
        int startI = 0;

        if (chatHistory.Count > messageTexts.Count)
            startI = chatHistory.Count - messageTexts.Count;

        for (int i = startI; i < chatHistory.Count; i++)
            messageTexts[i - startI].text = MessageToText(chatHistory[i]);

        lastShownMessageIndex = chatHistory.Count - 1;
    }

    private void UpdateView()
    {
        IReadOnlyList<Chat.Message> chatHistory = chat.History;

        if (chatHistory.Count <= 0 || lastShownMessageIndex >= chatHistory.Count - 1)
            return;

        if (IsChatFull())
        {
            UpdateShownMessages();
            return;
        }

        int messageTemplatesToSpawnCount = Mathf.Clamp(chatHistory.Count - messageTexts.Count, 0, maxTogetherShownMessages - messageTexts.Count);
        for (int i = 0; i < messageTemplatesToSpawnCount && !IsChatFull(); i++)
        {
            SpawnMessage(chatHistory[i]);
        }

        UpdateShownMessages();
    }

    private void UpdateNotificationView()
    {
        if (unreadMessagesCount > 0)
        {
            unreadMessagesCountText.text = unreadMessagesCount.ToString();
            unreadMessagesCountObject.SetActive(true);
        }
        else
            unreadMessagesCountObject.SetActive(false);
    }

    private void OnMessageReceived()
    {
        if (!isOpen)
        {
            unreadMessagesCount++;
            UpdateNotificationView();
        }

        UpdateView();
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        activeChatObject.SetActive(true);
        inactiveChatObject.SetActive(false);

        unreadMessagesCount = 0;
        UpdateNotificationView();
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;

        inactiveChatObject.SetActive(true);
        activeChatObject.SetActive(false);
    }

    public void ShowPainterObjects()
    {
        painterText.SetActive(true);
    }

    public void HidePainterObjects()
    {
        painterText.SetActive(false);
    }
}