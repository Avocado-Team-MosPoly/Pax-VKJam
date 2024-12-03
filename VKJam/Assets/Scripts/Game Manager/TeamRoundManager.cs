using System.Collections.Generic;

/// <summary> All logic on server </summary>
public class TeamRoundManager : RoundManager
{
    public TeamRoundManager(IGameManager gameManager, GameConfigSO gameConfig, IGuessSystem guessSystem, IngredientManager ingredientManager)
        : base(gameManager, gameConfig, guessSystem, ingredientManager) { }

    protected override void LoseRound()
    {
        base.LoseRound();

        int tokensToRemove = (int)(TokenManager.TokensCountWinnedCurrentRound * config.TokensMultiplyerIfRoundLosed_TM);
        TokenManager.RemoveTokensFromAll(tokensToRemove);
    }

    protected override void OnCorrectMonsterGuess(ulong clientId)
    {
        base.OnCorrectMonsterGuess(clientId);

        correctGuesserIds.Add(clientId);
        if (!correctGuesserIds.Contains(gameManager.PainterId))
            correctGuesserIds.Add(gameManager.PainterId);
    }

    protected override void OnWrongMonsterGuess(ulong clientId)
    {
        // Empty
    }

    protected override void WinRound()
    {
        base.WinRound();

        int tokensToAdd = 0;

        tokensToAdd += gameManager.IsDangerousCard ? config.BonusIfMonsterGuessed_TM_DM.GetValue(playersCount) * playersCount : config.BonusIfMonsterGuessed_TM_MM.GetValue(playersCount) * playersCount; // calculate bonus for monster type

        tokensToAdd += correctGuesserIds.Count >= 2 ? config.BonusIfMonsterGuessedMoreThanOnePlayer_TM.GetValue(playersCount) * playersCount : 0; // if more than 1 player guesses

        TokenManager.AddTokensToAll(tokensToAdd);
    }

    public override void OnTimeExpired()
    {
        IReadOnlyList<ulong> correctGuesserAllIds = ingredientManager.CorrectGuesserAllIds;
        Logger.Instance.Log(this, playersCount);
        Logger.Instance.LogWarning(this, correctGuesserAllIds.Count);

        // add logic for all ingredients guessed

        if (correctGuesserAllIds.Count > 1) // if more than 1 player guessed all
        {
            Logger.Instance.LogWarning(this, $"{correctGuesserAllIds.Count} player guessed all ingredients");
            int tokensToAdd = gameManager.IsDangerousCard ? config.BonusIfAllIngredientsGuessed_TM_DM.GetValue(playersCount) * playersCount : config.BonusIfAllIngredientsGuessed_TM_MM.GetValue(playersCount) * playersCount;
            TokenManager.AddTokensToAll(tokensToAdd);
        }

        base.OnTimeExpired();
    }
}