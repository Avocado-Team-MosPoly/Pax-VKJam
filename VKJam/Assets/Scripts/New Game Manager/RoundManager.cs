using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary> All logic on server </summary>
public abstract class RoundManager
{
    [HideInInspector] public UnityEvent OnRoundEnded = new();

    /// <summary> Correct guessed in this round </summary>
    protected List<ulong> correctGuesserIds = new();

    protected bool isMonsterGuessed;

    protected readonly IngredientManager ingredientManager;

    protected readonly GameConfigSO config;

    protected int playersCount => CustomNetworkManager.Singleton.ConnectedClientsIds.Count;

    public RoundManager(GameConfigSO gameConfig, CompareSystem compareSystem, IngredientManager ingredientManager)
    {
        config = gameConfig;
        this.ingredientManager = ingredientManager;

        compareSystem.OnMonsterGuess.AddListener(CompareMonster);
    }

    protected abstract void OnWrongMonsterGuess(ulong clientId);

    // called if one or more players guessed monster correctly
    protected virtual void WinRound()
    {
        Debug.Log("Round Win");
    }

    protected virtual void LoseRound()
    {
        Debug.Log("Round Losed");
    }

    protected virtual void OnCorrectMonsterGuess(ulong clientId)
    {
        isMonsterGuessed = true;
    }

    /// <summary> End of round </summary>
    public virtual void OnTimeExpired()
    {
        if (isMonsterGuessed)
            WinRound();
        else
            LoseRound();

        isMonsterGuessed = false;
        correctGuesserIds.Clear();

        OnRoundEnded?.Invoke();
    }

    public void CompareMonster(string guess, ulong guesserId)
    {
        string currentMonster = GameManager.Instance.GetCurrentMonster();

        Log($"Current Monster: {currentMonster}; Guess: {guess}; Guesser Id: {guesserId}");

        if (currentMonster.ToLower() == guess.ToLower())
            OnCorrectMonsterGuess(guesserId);
        else
            OnWrongMonsterGuess(guesserId);
    }

    public void PainterPenalty(int count)
    {
        if (count <=3)
        { TokenManager.RemoveTokensToClient(5, (byte)GameManager.Instance.PainterId); }
        else if (count ==4)
        { TokenManager.RemoveTokensToClient(3, (byte)GameManager.Instance.PainterId); }
        else if (count >=5)
        { TokenManager.RemoveTokensToClient(2, (byte)GameManager.Instance.PainterId); }
    }
    public void PainterReward(int count)
    {
        if (count <= 3)
        { TokenManager.AddTokensToClient(5, (byte)GameManager.Instance.PainterId); }
        else if (count == 4)
        { TokenManager.AddTokensToClient(3, (byte)GameManager.Instance.PainterId); }
        else if (count >= 5)
        { TokenManager.AddTokensToClient(2, (byte)GameManager.Instance.PainterId); }
    }

    private void Log(object message) => Debug.Log($"[{nameof(RoundManager)}] " + message);
}