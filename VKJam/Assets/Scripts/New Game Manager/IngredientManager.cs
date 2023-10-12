using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class IngredientManager : MonoBehaviour
{
    [SerializeField] private int tokensPerIngredient = 1;

    private bool isIngredientGuessed;
    public int CurrentIngredientIndex { get; private set; }
    private List<ulong> correctGuesserIds = new();
    private Dictionary<ulong, bool> correctGuesserAllIds = new();

    [HideInInspector] public UnityEvent<sbyte> OnIngredientSwitched;
    [HideInInspector] public UnityEvent OnIngredientsEnded;
    [HideInInspector] public UnityEvent OnCorrectIngredient;
    [HideInInspector] public UnityEvent OnWrongIngredient;

    private void Start()
    {
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

        if (CurrentIngredientIndex >= GameManager.Instance.AnswerCardSO.Ingredients.Length)
        {
            OnIngredientsEnded?.Invoke();
            CurrentIngredientIndex = 0;
            return;
        }

        OnIngredientSwitched?.Invoke((sbyte)CurrentIngredientIndex);
    }

    private void OnCorrectIngredientGuess(ulong clientId)
    {
        isIngredientGuessed = true;
        
        if (!GameManager.Instance.IsTeamMode)
        {
            correctGuesserIds.Add(clientId);
            if (!correctGuesserIds.Contains(GameManager.Instance.PainterId))
                correctGuesserIds.Add(GameManager.Instance.PainterId);
        }
    }

    private void OnWrongIngredientGuess(ulong clientId)
    {
        if (!GameManager.Instance.IsTeamMode)
        {
            if (correctGuesserIds.Contains(clientId))
            {
                correctGuesserIds.Remove(clientId);
                if (correctGuesserIds.Count == 1)
                {
                    correctGuesserIds.Clear();
                    isIngredientGuessed = false;
                }
            }
        }
    }

    private void CorrectIngredient()
    {
        Log("Correct guess");

        isIngredientGuessed = false;

        int playersCount = NetworkManager.Singleton.ConnectedClientsIds.Count;

        if (GameManager.Instance.IsTeamMode)
        {
            TokenManager.AddTokens(playersCount * 2);
        }
        else
        {
            foreach (byte clientId in correctGuesserIds)
            {
                TokenManager.AddTokensToClient(1, clientId);
            }

            foreach (ulong clientId in correctGuesserAllIds.Keys)
            {
                if (!correctGuesserIds.Contains(clientId))
                    correctGuesserAllIds[clientId] = false;
            }

            correctGuesserIds.Clear();
        }

        OnCorrectIngredient?.Invoke();
    }

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

    private void Log(object message) => Debug.Log($"[{name}] " + message);
}