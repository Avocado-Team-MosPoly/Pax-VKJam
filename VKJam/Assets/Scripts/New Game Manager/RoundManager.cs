using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private int tokensPerRound = 5;

    private bool isMonsterGuessed;
    private List<ulong> correctGuesserIds = new List<ulong>();

    [HideInInspector] public UnityEvent OnRoundEnded;

    private void OnWrongMonsterGuess(ulong clientId)
    {
        if (!GameManager.Instance.IsTeamMode)
        {
            if (correctGuesserIds.Contains(clientId))
            {
                correctGuesserIds.Remove(clientId);
                if (correctGuesserIds.Count == 1)
                {
                    correctGuesserIds.Clear();
                    isMonsterGuessed = false;
                }
            }
        }
    }

    private void OnCorrectMonsterGuess(ulong clientId)
    {
        isMonsterGuessed = true;

        if (!GameManager.Instance.IsTeamMode)
        {
            correctGuesserIds.Add(clientId);
            if (!correctGuesserIds.Contains(GameManager.Instance.PainterId))
                correctGuesserIds.Add(GameManager.Instance.PainterId);
        }
    }

    private void WinRound()
    {
        Log("Win Round");

        isMonsterGuessed = false;

        int playersCount = NetworkManager.Singleton.ConnectedClientsIds.Count;

        if (GameManager.Instance.IsTeamMode)
        {
            int tokensToAdd = playersCount * playersCount * 4;

            if (GameManager.Instance.AnswerCardSO.Difficulty == CardDifficulty.Dangerous)
                TokenManager.AddTokens(tokensToAdd);
            else
                TokenManager.AddTokens((int)(tokensToAdd * 1.5f));
            
            if (playersCount - 2 > 0)
                TokenManager.AddTokens(playersCount);
        }
        else
        {
            PainterPenalty(correctGuesserIds.Count);
            PainterReward(correctGuesserIds.Count);
            if (GameManager.Instance.AnswerCardSO.Difficulty == CardDifficulty.Dangerous)
            {
                foreach (byte clientId in correctGuesserIds)
                {
                    if (clientId != GameManager.Instance.PainterId)
                        TokenManager.AddTokensToClient(4, clientId);
                    else
                    {
                        int tokensToAdd = 4;
                        if (correctGuesserIds.Count > 2)
                        {
                            tokensToAdd += playersCount - 1;
                        }

                        TokenManager.AddTokensToClient(tokensToAdd, clientId);
                    }
                }
            }
            else
            {
                foreach (byte clientId in correctGuesserIds)
                {
                    if (clientId != GameManager.Instance.PainterId)
                        TokenManager.AddTokensToClient(6, clientId);
                    else
                    {
                        int tokensToAdd = 6;
                        if (correctGuesserIds.Count > 2)
                        {
                            tokensToAdd += playersCount - 1;
                        }

                        TokenManager.AddTokensToClient(tokensToAdd, clientId);
                    }
                }
            }
            correctGuesserIds.Clear();
        }
    }

    private void LoseRound()
    {
        Log("Lose Round");

        if (GameManager.Instance.IsTeamMode)
        {
            int tokensToRemove = (int)(TokenManager.TokensCountWinnedCurrentRound * 0.6f);
            TokenManager.RemoveTokens(tokensToRemove);
        }
        else
        {
            PainterPenalty(correctGuesserIds.Count);
            PainterReward(correctGuesserIds.Count);
        }
        
        isMonsterGuessed = false;
        correctGuesserIds.Clear();
    }

    public void OnTimeExpired()
    {
        if (isMonsterGuessed)
            WinRound();
        else
            LoseRound();

        OnRoundEnded?.Invoke();
    }

    public void CompareMonster(string guess, ulong guesserId)
    {
        string currentMonster = GameManager.Instance.GetCurrentMonster();

        Log($"Current Monster: {currentMonster}, Guess: {guess}, Guesser Id: {guesserId}");

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

    private void Log(object message) => Debug.Log($"[{name}] " + message);
}