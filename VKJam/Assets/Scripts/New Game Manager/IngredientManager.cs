using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public abstract class IngredientManager
{
    public UnityEvent<sbyte> OnIngredientSwitched = new();
    public UnityEvent OnIngredientsEnded = new();
    public UnityEvent OnCorrectIngredient = new();
    public UnityEvent OnWrongIngredient = new();

    public int CurrentIngredientIndex { get; private set; }

    public IReadOnlyList<ulong> CorrectGuesserAllIds
    {
        get
        {
            List<ulong> result = new();

            foreach (ulong clientId in correctGuesserAllIds.Keys)
                if (correctGuesserAllIds[clientId])
                    result.Add(clientId);

            return result;
        }
    }

    protected List<ulong> correctGuesserIds = new();

    /// <summary> Players guessed without mistakes </summary>
    protected Dictionary<ulong, bool> correctGuesserAllIds = new();

    protected bool isIngredientGuessed;

    protected readonly GameConfigSO config;
    
    protected int playersCount => NetworkManager.Singleton.ConnectedClientsIds.Count;

    public IngredientManager(GameConfigSO config, CompareSystem compareSystem)
    {
        if (NetworkManager.Singleton == null)
            throw new System.Exception("Ingredient Manager needs Network Manager instance on scene");

        this.config = config;
        compareSystem.OnIngredientGuess.AddListener(CompareIngredient);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            correctGuesserAllIds[clientId] = true;

        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) =>
        {
            correctGuesserAllIds[clientId] = GameManager.Instance.CurrentRound == 1;
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) =>
        {
            correctGuesserAllIds.Remove(clientId);
        };
    }

    private void NextIngredient()
    {
        CurrentIngredientIndex++;

        if (CurrentIngredientIndex >= GameManager.Instance.IngredientsCount)
        {
            OnIngredientsEnded?.Invoke();
            CurrentIngredientIndex = 0;
            return;
        }

        OnIngredientSwitched?.Invoke((sbyte)CurrentIngredientIndex);
    }

    protected virtual void OnCorrectIngredientGuess(ulong clientId)
    {
        isIngredientGuessed = true;
    }

    protected virtual void OnWrongIngredientGuess(ulong clientId)
    {

    }

    protected abstract void CorrectIngredient();

    private void WrongIngredient()
    {
        Log("Wrong Ingredient");

        isIngredientGuessed = false;

        //TokenManager.RemoveTokens(1);

        OnWrongIngredient?.Invoke();
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
            OnCorrectIngredientGuess(guesserId);
        else
            OnWrongIngredientGuess(guesserId);
    }

    protected void Log(object message) => Debug.Log($"[{nameof(IngredientManager)}] " + message);
}