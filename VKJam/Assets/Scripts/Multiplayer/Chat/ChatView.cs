using TMPro;
using UnityEngine;

[RequireComponent(typeof(Chat))]
public class ChatView : MonoBehaviour
{
    [SerializeField] private RectTransform chatContainer;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject showMoreButtonPrefab;

    [SerializeField] private GameObject notificationObject;
    [SerializeField] private TextMeshProUGUI notificationText;

    [SerializeField, Tooltip("Is not recommended set more than 50")] private int maxTogetherShownMessages = 20;

    private Chat chat;

    private Chat.Message lastShownMessage = new()
    {
        text = "!empty!"
    };

    private void Awake()
    {
        chat = GetComponent<Chat>();

        chat.MessageSended.AddListener(OnMessageSended);
        chat.MessageReceived.AddListener(OnMessageReceived);
    }

    private void OnMessageReceived()
    {
        UpdateChatView();
    }

    private void OnMessageSended(Chat.Message message)
    {
        UpdateChatView();
    }

    private void UpdateChatView()
    {
        if (lastShownMessage.text == "!empty!" || lastShownMessage == chat.History[chat.History.Count - 1])
            return;

        for (int i = 0; i < chat.History.Count; i++)
        {
            GameObject messageInstance = Instantiate(messagePrefab, chatContainer);
            TextMeshProUGUI textField = messageInstance.GetComponent<TextMeshProUGUI>();

            textField.text = chat.History[i].senderId + ": " + chat.History[i].text;
        }

        lastShownMessage = chat.History[chat.History.Count - 1];
    }
}