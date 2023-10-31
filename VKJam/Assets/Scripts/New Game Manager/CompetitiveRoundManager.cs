using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class CompetitiveRoundManager : RoundManager
{
    public CompetitiveRoundManager(GameConfigSO gameConfig, CompareSystem compareSystem, IngredientManager ingredientManager)
        : base(gameConfig, compareSystem, ingredientManager) { }

    protected override void LoseRound()
    {
        base.LoseRound();

        foreach (byte clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != GameManager.Instance.PainterId)
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

        int baseTokensIfMonsterGuessed_G = GameManager.Instance.IsDangerousCard ?
            config.BonusIfMonsterGuessed_CM_DM_G.GetValue(playersCount) : config.BonusIfMonsterGuessed_CM_MM_G.GetValue(playersCount);
        int baseTokensIfMonsterGuessed_P = GameManager.Instance.IsDangerousCard ?
            config.BonusIfMonsterGuessed_CM_DM_P.GetValue(playersCount) : config.BonusIfMonsterGuessed_CM_MM_P.GetValue(playersCount);
        // guesser params
        int tokensIfAllIngredientsGuessed = GameManager.Instance.IsDangerousCard ?
            config.BonusIfAllIngredientsGuessed_TM_DM.GetValue(playersCount) : config.BonusIfAllIngredientsGuessed_TM_MM.GetValue(playersCount);
        // painter params
        int tokensIfMoreThanOnePlayerGuessedMonster_P = config.BonusIfMonsterGuessedMoreThanOnePlayer_CM_P.GetValue(playersCount) * correctGuesserAllIds.Count;

        //if (correctGuesserIds.Count > 2)
        //    tokensIfMoreThanOnePlayerGuessedMonster += (playersCount - 1) * (correctGuesserIds.Count - 2);

        foreach (byte clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (correctGuesserIds.Contains(clientId))
            {
                if (clientId != GameManager.Instance.PainterId) // if client is not painter
                {
                    int tokensToAdd_G = baseTokensIfMonsterGuessed_G;

                    if (correctGuesserAllIds.Contains(clientId))
                        tokensToAdd_G += tokensIfAllIngredientsGuessed;

                    TokenManager.AddTokensToClient(tokensToAdd_G, clientId);
                }
                else  // if client is painter
                {
                    int tokensToAdd_P = baseTokensIfMonsterGuessed_P + tokensIfMoreThanOnePlayerGuessedMonster_P;

                    TokenManager.AddTokensToClient(tokensToAdd_P, clientId);
                }
            }
            else
            {
                if (clientId != GameManager.Instance.PainterId)
                    TokenManager.RemoveTokensToClient(config.PenaltyIfMonsterIsNotGuessed_CM_G.GetValue(playersCount), clientId);
            }
        }

        int tokensToRemove = CalculatePenalty(correctGuesserIds.Count);

        TokenManager.RemoveTokensToClient(tokensToRemove, GameManager.Instance.PainterId);

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