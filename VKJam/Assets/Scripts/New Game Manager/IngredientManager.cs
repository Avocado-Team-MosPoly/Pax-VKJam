using UnityEngine;
using UnityEngine.Events;

public class IngredientManager : MonoBehaviour
{
    private bool isIngredientGuessed;
    public int CurrentIngredientIndex { get; protected set; }

    [HideInInspector] public UnityEvent<string> OnIngredientSwitched;
    [HideInInspector] public UnityEvent OnIngredientsEnded;

    private void NextIngredient()
    {
        CurrentIngredientIndex++;

        if (CurrentIngredientIndex >= GameManager.Instance.AnswerCardSO.Ingredients.Length)
        {
            OnIngredientsEnded?.Invoke();
            return;
        }

        OnIngredientSwitched?.Invoke(GameManager.Instance.GetCurrentIngredient());
    }

    private void OnCorrectIngredientGuess()
    {
        isIngredientGuessed = true;
    }

    private void CorrectIngredient()
    {
        Log("Correct guess");

        isIngredientGuessed = false;
    }

    private void WrongIngredient()
    {
        Log("Wrong Ingredient");

        isIngredientGuessed = false;
    }

    public void OnTimeExpired()
    {
        if (isIngredientGuessed)
            CorrectIngredient();
        else
            WrongIngredient();

        NextIngredient();
    }

    public void CompareIngredient(string guess, ulong guesserId)
    {
        string currentIngredient = GameManager.Instance.GetCurrentIngredient();

        Log($"Current Ingredient: {currentIngredient}, Guess: {guess}, Guesser Id: {guesserId}");

        if (currentIngredient.ToLower() == guess.ToLower())
            OnCorrectIngredientGuess();
    }

    private void Log(object message) => Debug.Log($"[{name}] " + message);
}