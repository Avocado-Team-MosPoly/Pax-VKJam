using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngredientInfo : MonoBehaviour
{
    public TextMeshProUGUI IngridientName;
    [HideInInspector] public UnityEvent<int> OnGuess;

    private FirstModeGuessSystem compareSystem;
    private int index;

    private Button closeBestiaryButton;

    public void SetIngridient(string name, int index, FirstModeGuessSystem compareSystem, Button close)
    {
        IngridientName.text = name;
        this.index = index;
        this.compareSystem = compareSystem;
        
        gameObject.SetActive(true);
        closeBestiaryButton = close;

        GetComponent<Button>().onClick.AddListener(Guess);
    }

    public void Guess()
    {
        if (GameManager.Instance.Stage == Stage.IngredientGuess)
        {
            compareSystem.SendAnswerServerRpc(index, new ServerRpcParams());
            OnGuess?.Invoke(index);
            closeBestiaryButton.OnPointerClick(new PointerEventData(EventSystem.current));
        }
    }
}
