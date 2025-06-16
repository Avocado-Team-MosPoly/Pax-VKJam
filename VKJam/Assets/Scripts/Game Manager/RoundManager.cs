using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary> All logic on server </summary>
public abstract class RoundManager
{
    [HideInInspector] public UnityEvent OnRoundEnded = new();
    [HideInInspector] public UnityEvent OnMonsterGuessed = new();
    [HideInInspector] public UnityEvent OnMonsterNotGuessed = new();

    public IReadOnlyList<ulong> CorrectGuesserIds => correctGuesserIds;

    /// <summary> Correct guessed in this round </summary>
    protected List<ulong> correctGuesserIds = new();
    private List<ulong> guesserIds = new();

    protected bool isMonsterGuessed;

    protected readonly IngredientManager ingredientManager;

    protected readonly IGameManager gameManager;
    protected readonly GameConfigSO config;

    protected int playersCount => NetworkManager.Singleton.ConnectedClientsIds.Count;

    public RoundManager(IGameManager gameManager, GameConfigSO gameConfig, IGuessSystem guessSystem, IngredientManager ingredientManager)
    {
        this.gameManager = gameManager;
        config = gameConfig;
        this.ingredientManager = ingredientManager;

        guessSystem.OnMonsterGuess.AddListener(CompareMonster);
    }

    protected abstract void OnWrongMonsterGuess(ulong clientId);

    // called if one or more players guessed monster correctly
    protected virtual void WinRound()
    {
        Log("Round Win");
        OnMonsterGuessed?.Invoke();
        //GameManager.Instance.SceneMonsterAnimator.Play("Win");
    }

    protected virtual void LoseRound()
    {
        Log("Round Losed");
        OnMonsterNotGuessed?.Invoke();
        //GameManager.Instance.SceneMonsterAnimator.Play("Loose");
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
        guesserIds.Clear();
        
        OnRoundEnded?.Invoke();

        correctGuesserIds.Clear();
    }

    public void CompareMonster(string guess, ulong guesserId)
    {
        string currentMonster = gameManager.CurrentMonsterName;

        Log($"Current Monster: {currentMonster}; Guess: {guess}; Guesser Id: {guesserId}");

        if (currentMonster.ToLower() == guess.ToLower())
            OnCorrectMonsterGuess(guesserId);
        else
            OnWrongMonsterGuess(guesserId);

        if (guesserIds.IndexOf(guesserId) == -1)
            guesserIds.Add(guesserId);

        if (guesserIds.Count >= playersCount - 1)
            GameManager.Instance.AllPlayersGuessed(); // TODO: ??
    }

    private void Log(object message) => Logger.Instance.Log(this, message);
}