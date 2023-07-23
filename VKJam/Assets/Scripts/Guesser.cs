using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Guesser : MonoBehaviour
{
    [SerializeField] private CardManager cardManager;

    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject[] gameObjects;

    [SerializeField] private TMP_InputField guessInputField;
    [SerializeField] private Button guessButton;
    private string guess;
    private bool isMonsterGuessStage = false;

    private void Start()
    {
        guessInputField.onValueChanged.AddListener(ChangeGuess);
        guessButton.onClick.AddListener(SubmitGuess);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) && guessButton.gameObject.activeInHierarchy)
        {
            SubmitGuess();
        }
    }

    private void ChangeGuess(string value)
    {
        guess = value;
    }

    private void SubmitGuess()
    {
        if (guess == string.Empty)
            return;

        if (isMonsterGuessStage)
            SubmitMonster();
        else
            SubmitIngredient();
    }

    private void SubmitIngredient()
    {
        GameManager.Instance.CompareIngredient(guess);
        guessInputField.text = string.Empty;
    }

    private void SubmitMonster()
    {
        CardSO cardSO = cardManager.GetCardSOById(guess);
        GameManager.Instance.CompareMonster(cardSO);

        isMonsterGuessStage = false;
        guessInputField.text = string.Empty;
    }

    public void SetMonsterStage()
    {
        isMonsterGuessStage = true;
    }

    public void SetMonsterGuessStage()
    {
        isMonsterGuessStage = true;
    }

    public void Activate()
    {
        foreach (GameObject obj in gameObjects)
            obj.SetActive(true);
    }

    public void Deactivate()
    {
        foreach (GameObject obj in gameObjects)
            obj.SetActive(false);
    }

    public void DeactivateUI()
    {
        UI.SetActive(false);
    }
}