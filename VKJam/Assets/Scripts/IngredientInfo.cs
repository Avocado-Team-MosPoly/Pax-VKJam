using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngredientInfo : MonoBehaviour
{
    public TextMeshProUGUI IngridientName;

    public UnityEvent<int> OnGuess { get; private set; } = new();

    private FirstModeGuessSystem guessSystem;
    private int index;

    private Button closeBestiaryButton;

    public void SetIngredient(string name, int index, FirstModeGuessSystem guessSystem, Button closeButton)
    {
        IngridientName.text = name;
        this.index = index;
        this.guessSystem = guessSystem;
        
        gameObject.SetActive(true);
        closeBestiaryButton = closeButton;

        GetComponent<Button>().onClick.AddListener(Guess);
    }

    public void Guess()
    {
        if (GameManager.Instance.Stage == Stage.IngredientGuess)
        {
            guessSystem.SendAnswerServerRpc(index, new ServerRpcParams());
            OnGuess?.Invoke(index);
            closeBestiaryButton.OnPointerClick(new PointerEventData(EventSystem.current));
        }
    }
}