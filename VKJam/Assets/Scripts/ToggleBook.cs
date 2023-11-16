using UnityEngine;
using UnityEngine.UI;

public class ToggleBook : MonoBehaviour
{
    [SerializeField] private KeyCode toggleBookKey = KeyCode.Tab;

    [Header("Painter")]
    [SerializeField] private Interactable painterBook;
    [SerializeField] private Button painterBookCloseButton;
    
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
            if (painterBookCloseButton.gameObject.activeInHierarchy)
                painterBookCloseButton.onClick?.Invoke();
            else
                painterBook.m_OnClick?.Invoke();
        }
        else
        {
            if (guesserBookCloseButton.gameObject.activeInHierarchy)
                guesserBookCloseButton.onClick?.Invoke();
            else
                guesserBook.m_OnClick?.Invoke();
        }
    }
}