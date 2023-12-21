using UnityEngine;
using UnityEngine.UI;

public class ToggleBook : MonoBehaviour
{
    [SerializeField] private KeyCode toggleBookKey = KeyCode.Tab;

    [Header("Painter")]
    [SerializeField] private Interactable painterBook;
    [SerializeField] private SpriteButton painterBookCloseButton;
    
    [Header("Guesser")]
    [SerializeField] private Interactable guesserBook;
    [SerializeField] private Button guesserBookCloseButton;

    private void Update()
    {
        if (Input.GetKeyDown(toggleBookKey))
            Toggle();
    }

    private void Toggle()
    {
        if (GameManager.Instance.IsPainter)
        {
            Logger.Instance.Log(this, painterBookCloseButton.gameObject.activeInHierarchy);
            if (painterBookCloseButton.gameObject.activeInHierarchy)
                painterBookCloseButton.OnClick?.Invoke();
            else
                painterBook.m_OnClick?.Invoke();
        }
        else
        {
            Logger.Instance.Log(this, "Is Painter: " + GameManager.Instance.IsPainter + " active: " + guesserBookCloseButton.gameObject.activeInHierarchy);
            if (guesserBookCloseButton.gameObject.activeInHierarchy)
                guesserBookCloseButton.onClick?.Invoke();
            else
                guesserBook.m_OnClick?.Invoke();
        }
    }
}