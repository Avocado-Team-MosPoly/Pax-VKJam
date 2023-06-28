using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Guesser : MonoBehaviour
{
    [SerializeField] private TMP_InputField guessInputField;
    [SerializeField] private Button guessButton;

    private string guess;

    private void Start()
    {
        guessInputField.onValueChanged.AddListener(ChangeGuess);
        guessButton.onClick.AddListener(SubmitGuess);
    }

    private void ChangeGuess(string value)
    {
        guess = value;
    }

    private void SubmitGuess()
    {
        GameManager.Instance.CompareIngredient(guess);
    }
}