using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

/// <summary> All logic on server </summary>
public abstract class IngredientManager
{
    public UnityEvent<sbyte> OnIngredientSwitched = new();
    public UnityEvent OnIngredientsEnded = new();
    public UnityEvent OnCorrectIngredient = new();
    public UnityEvent OnWrongIngredient = new();

    private int currentIngredientIndex = 0;
    private List<int> ingredientIndexes = new();
    public int GetCurrentIngredientIndex => ingredientIndexes[currentIngredientIndex];

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
    protected Dictionary<ulong, int> failedGuesses = new();

    protected bool isIngredientGuessed;

    protected IGameManager gameManager;
    protected readonly GameConfigSO config;
    
    protected int playersCount => NetworkManager.Singleton.ConnectedClientsIds.Count;

    public IngredientManager(IGameManager gameManager, GameConfigSO config, IGuessSystem guessSystem)
    {
        if (NetworkManager.Singleton == null)
            throw new System.Exception("Ingredient Manager needs Network Manager instance on scene");

        this.gameManager = gameManager;
        this.config = config;

        guessSystem.OnIngredientGuess.AddListener(CompareIngredient);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            correctGuesserAllIds[clientId] = true;

        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) =>
        {
            correctGuesserAllIds[clientId] = gameManager.CurrentRound == 1;
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) =>
        {
            correctGuesserAllIds.Remove(clientId);
        };
    }

    private void NextIngredient()
    {
        currentIngredientIndex++;

        if (currentIngredientIndex >= gameManager.IngredientsCount)
        {
            OnIngredientsEnded?.Invoke();
            currentIngredientIndex = 0;
            return;
        }

        OnIngredientSwitched?.Invoke((sbyte)ingredientIndexes[currentIngredientIndex]);
    }

    protected virtual void OnCorrectIngredientGuess(ulong clientId)
    {
        isIngredientGuessed = true;

        if (failedGuesses.ContainsKey(clientId) && failedGuesses[clientId] == currentIngredientIndex)
        {
            Logger.Instance.LogWarning(this, "Removed wrong ingredient");
            failedGuesses.Remove(clientId);
            correctGuesserAllIds[clientId] = true;

            if (CorrectGuesserAllIds.Count > 0 && correctGuesserAllIds[gameManager.PainterId] == false)
            {
                correctGuesserAllIds[gameManager.PainterId] = true;
            }
        }
    }

    protected virtual void OnWrongIngredientGuess(ulong clientId)
    {
        if (!failedGuesses.ContainsKey(clientId))
        {
            Logger.Instance.LogWarning(this, "Added new wrong ingredient");
            failedGuesses.Add(clientId, currentIngredientIndex);
        }
        else
        {
            Logger.Instance.LogWarning(this, "Changed wrong ingredient");
            failedGuesses[clientId] = currentIngredientIndex;
        }

        correctGuesserAllIds[clientId] = false;

        if(CorrectGuesserAllIds.Count == 1 && CorrectGuesserAllIds[0] == gameManager.PainterId)
        {
            correctGuesserAllIds[gameManager.PainterId] = false;
        }
    }

    protected abstract void CorrectIngredient();

    private void WrongIngredient()
    {
        Log("Wrong Ingredient");

        isIngredientGuessed = false;

        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!correctGuesserIds.Contains(clientId))
            {
                OnWrongIngredientGuess(clientId);
            }
        }

        OnWrongIngredient?.Invoke();
    }

    public void OnTimeExpired()
    {
        if (isIngredientGuessed)
            CorrectIngredient();
        else
            WrongIngredient();

        correctGuesserIds.Clear();
        NextIngredient();
    }

    public void CompareIngredient(string guess, ulong guesserId)
    {
        string currentIngredient = gameManager.CurrentIngredientName;

        Log($"Current Ingredient: {currentIngredient}, Guess: {guess}, Guesser Id: {guesserId}");

        if (currentIngredient.ToLower() == guess.ToLower())
            OnCorrectIngredientGuess(guesserId);
        else
            OnWrongIngredientGuess(guesserId);
    }

    public void SetIngredients(int count)
    {
        ingredientIndexes.Clear();

        List<int> temp;

        if (count > 4)
        {
            temp = new List<int> { 0, 1, 2, 3, 4 };
        }
        else
        {
            temp = new List<int> { 0, 1, 2, 3 };
        }
        

        for (int i = 0; i < count; i++)
        {
            var random = Random.Range(0, temp.Count);
            var randomValue = temp[random];
            temp.RemoveAt(random);
            ingredientIndexes.Add(randomValue);
        }
    }

    protected void Log(object message) => Debug.Log($"[{nameof(IngredientManager)}] " + message);
}