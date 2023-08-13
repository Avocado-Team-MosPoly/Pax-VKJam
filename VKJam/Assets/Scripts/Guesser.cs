using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Guesser : MonoBehaviour
{
    [SerializeField] private TMP_InputField guessInputField;
    [SerializeField] private Button guessButton;

    private bool isIngredientStage = true;

    private string guess;

    private void Start()
    {
        guessInputField.onValueChanged.AddListener(ChangeGuess);
        guessButton.onClick.AddListener(SubmitGuess);

        GameManager.Instance.OnWinRound.AddListener(OnIngredientStageStart);
        GameManager.Instance.OnIngredientsEnd.AddListener(OnMonsterStageStart);
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.KeypadEnter) && guessButton.gameObject.activeInHierarchy)
    //    {
    //        SubmitGuess();
    //    }
    //}

    private void OnIngredientStageStart()
    {
        isIngredientStage = true;
        Debug.Log("OnIngredientStageStart");
    }

    private void OnMonsterStageStart()
    {
        isIngredientStage = false;
        Debug.Log("OnMonsterStageStart");
    }

    private void ChangeGuess(string value)
    {
        guess = value;
    }

    private void SubmitGuess()
    {
        if (guess == string.Empty)
            return;

        Debug.Log("is ingredient stage : " + isIngredientStage);

        if (isIngredientStage)
            GameManager.Instance.CompareIngredient(guess);
        else
            GameManager.Instance.CompareMonster(guess);


        guessInputField.text = string.Empty;
    }
}