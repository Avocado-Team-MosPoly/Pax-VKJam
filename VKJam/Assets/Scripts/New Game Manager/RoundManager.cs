using UnityEngine;
using UnityEngine.Events;

public class RoundManager : MonoBehaviour
{
    private bool isMonsterGuessed;

    [HideInInspector] public UnityEvent OnRoundWon;
    [HideInInspector] public UnityEvent OnRoundLosed;
    [HideInInspector] public UnityEvent OnRoundEnded;

    private void OnCorrectMonsterGuess()
    {
        isMonsterGuessed = true;
    }

    private void WinRound()
    {
        Log("Win Round");

        isMonsterGuessed = false;
        OnRoundWon?.Invoke();
    }

    private void LoseRound()
    {
        Log("Lose Round");

        isMonsterGuessed = false;
        OnRoundLosed?.Invoke();
    }

    public void OnTimeExpired()
    {
        if (isMonsterGuessed)
            WinRound();
        else
            LoseRound();
    }

    public void CompareMonster(string guess, ulong guesserId)
    {
        string currentMonster = GameManager.Instance.GetCurrentMonster();

        Log($"Current Monster: {currentMonster}, Guess: {guess}, Guesser Id: {guesserId}");

        if (currentMonster == guess)
            OnCorrectMonsterGuess();
    }

    private void Log(object message) => Debug.Log($"[{name}] " + message);
}