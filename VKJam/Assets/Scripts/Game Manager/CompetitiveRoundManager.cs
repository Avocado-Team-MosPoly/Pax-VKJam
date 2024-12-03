using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

/// <summary> All logic on server </summary>
public class CompetitiveRoundManager : RoundManager
{
    public CompetitiveRoundManager(IGameManager gameManager, GameConfigSO gameConfig, IGuessSystem guessSystem, IngredientManager ingredientManager)
        : base(gameManager, gameConfig, guessSystem, ingredientManager) { }

    protected override void LoseRound()
    {
        base.LoseRound();

        foreach (byte clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != gameManager.PainterId)
                TokenManager.RemoveTokensToClient(config.PenaltyIfMonsterIsNotGuessed_CM_G.GetValue(playersCount), clientId);
            else
            {
                int tokensToRemove_P = CalculatePenalty(0);
                TokenManager.RemoveTokensToClient(tokensToRemove_P, clientId);
            }
        }

        correctGuesserIds.Clear();
    }

    protected override void OnWrongMonsterGuess(ulong clientId)
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

    protected override void WinRound()
    {
        base.WinRound();

        //PainterPenalty(correctGuesserIds.Count);
        //PainterReward(correctGuesserIds.Count);

        IReadOnlyList<ulong> correctGuesserAllIds = ingredientManager.CorrectGuesserAllIds;
        Logger.Instance.LogWarning(this, $"{correctGuesserAllIds.Count} player guessed all ingredients");

        int baseTokensIfMonsterGuessed_G;
        int baseTokensIfMonsterGuessed_P;
        int tokensIfAllIngredientsGuessed_G;
        int tokensIfAllIngredientsGuessed_P; // use this value
        int tokensIfMoreThanOnePlayerGuessedMonster_P = correctGuesserIds.Count > 1 ? config.BonusIfMonsterGuessedMoreThanOnePlayer_CM_P.GetValue(correctGuesserIds.Count) : 0; // check condition

        if (gameManager.IsDangerousCard)
        {
            baseTokensIfMonsterGuessed_G = config.BonusIfMonsterGuessed_CM_DM_G.GetValue(playersCount);
            baseTokensIfMonsterGuessed_P = config.BonusIfMonsterGuessed_CM_DM_P.GetValue(playersCount);
            tokensIfAllIngredientsGuessed_G = config.BonusIfAllIngredientsGuessed_CM_DM_G.GetValue(playersCount);
            tokensIfAllIngredientsGuessed_P = config.BonusIfAllIngredientsGuessed_CM_DM_P.GetValue(playersCount);
        }
        else
        {
            baseTokensIfMonsterGuessed_G = config.BonusIfMonsterGuessed_CM_MM_G.GetValue(playersCount);
            baseTokensIfMonsterGuessed_P = config.BonusIfMonsterGuessed_CM_MM_P.GetValue(playersCount);
            tokensIfAllIngredientsGuessed_G = config.BonusIfAllIngredientsGuessed_CM_MM_G.GetValue(playersCount);
            tokensIfAllIngredientsGuessed_P = config.BonusIfAllIngredientsGuessed_CM_MM_P.GetValue(playersCount);
        }

        //if (correctGuesserIds.Count > 2)
        //    tokensIfMoreThanOnePlayerGuessedMonster += (playersCount - 1) * (correctGuesserIds.Count - 2);

        foreach (byte clientId in NetworkManager.Singleton.ConnectedClientsIds) // logic for host ??
        {
            int tokesToAdd = 0;

            if (correctGuesserIds.Contains(clientId)) // guessed monster
            {
                if (clientId != gameManager.PainterId) // guesser
                {
                    tokesToAdd += baseTokensIfMonsterGuessed_G;
                    tokesToAdd += correctGuesserAllIds.Contains(clientId) ? tokensIfAllIngredientsGuessed_G : 0; // add logic for all ingredients guessed
                }
                else // painter
                {
                    tokesToAdd += (baseTokensIfMonsterGuessed_P + tokensIfMoreThanOnePlayerGuessedMonster_P);
                    tokesToAdd += (UnityEngine.Mathf.Max(0, correctGuesserIds.Count - 1) * tokensIfAllIngredientsGuessed_P);
                }
            }
            else // missed monster
            {
                if (clientId != gameManager.PainterId) // guesser
                {
                    tokesToAdd += correctGuesserAllIds.Contains(clientId) ? tokensIfAllIngredientsGuessed_G : 0; // add logic for all ingredients guessed
                    TokenManager.RemoveTokensToClient(config.PenaltyIfMonsterIsNotGuessed_CM_G.GetValue(playersCount), clientId);
                }
            }

            TokenManager.AddTokensToClient(tokesToAdd, clientId);
        }

        int tokensToRemovePainter = CalculatePenalty(correctGuesserIds.Count);

        TokenManager.RemoveTokensToClient(tokensToRemovePainter, gameManager.PainterId);

        correctGuesserIds.Clear();
    }

    private int CalculatePenalty(int correctGuesserIdsCount)
    {
        int wrongGuessedPlayers =
            NetworkManager.Singleton.ConnectedClientsIds.Count -
            (correctGuesserIdsCount == 0 ? 1 : correctGuesserIdsCount);

        return config.PenaltyIfMonsterIsNotGuessed_CM_P.GetValue(playersCount) * wrongGuessedPlayers;
    }
}