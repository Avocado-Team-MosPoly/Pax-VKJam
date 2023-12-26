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
        currentIngredientIndex++;

        if (currentIngredientIndex >= GameManager.Instance.IngredientsCount)
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
    }

    protected virtual void OnWrongIngredientGuess(ulong clientId) //?
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

    public void SetIngredients(int count)
    {
        ingredientIndexes.Clear();

        List<int> temp;

        if (count > 4.5)
        {
            temp = new List<int> { 0, 1, 2, 3, 4 };
        }
        else
        {
            temp = new List<int> { 0, 1, 2, 3 };
        }
        

        for(int i = 0; i < count; i++)
        {
            var random = Random.Range(0, temp.Count);
            var randomValue = temp[random];
            temp.RemoveAt(random);
            ingredientIndexes.Add(randomValue);
        }

        foreach(var index in ingredientIndexes)
        {
            Debug.LogWarning(index);
        }
    }

    protected void Log(object message) => Debug.Log($"[{nameof(IngredientManager)}] " + message);
}