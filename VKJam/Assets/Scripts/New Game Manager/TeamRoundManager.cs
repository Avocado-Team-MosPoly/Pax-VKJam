using System.Collections.Generic;

/// <summary> All logic on server </summary>
public class TeamRoundManager : RoundManager
{
    public TeamRoundManager(GameConfigSO gameConfig, CompareSystem compareSystem, IngredientManager ingredientManager)
        : base(gameConfig, compareSystem, ingredientManager) { }

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
        if (!correctGuesserIds.Contains(GameManager.Instance.PainterId))
            correctGuesserIds.Add(GameManager.Instance.PainterId);
    }

    protected override void OnWrongMonsterGuess(ulong clientId)
    {
        // Empty
    }

    protected override void WinRound()
    {
        base.WinRound();

        if (GameManager.Instance.IsDangerousCard) // if monster is dangerous (common)
            TokenManager.AddTokensToAll(config.BonusIfMonsterGuessed_TM_DM.GetValue(playersCount));
        else  // if monster is murderous (hard)
            TokenManager.AddTokensToAll(config.BonusIfMonsterGuessed_TM_MM.GetValue(playersCount));

        if (correctGuesserIds.Count > 2)
            TokenManager.AddTokensToAll(config.BonusIfMonsterGuessedMoreThanOnePlayer_TM.GetValue(playersCount));
    }

    public override void OnTimeExpired()
    {
        IReadOnlyList<ulong> correctGuesserAllIds = ingredientManager.CorrectGuesserAllIds;
        Logger.Instance.Log(playersCount);
        int tokensToAdd = GameManager.Instance.IsDangerousCard ?
            correctGuesserAllIds.Count * config.BonusIfAllIngredientsGuessed_TM_DM.GetValue(playersCount) :
            correctGuesserAllIds.Count * config.BonusIfAllIngredientsGuessed_TM_MM.GetValue(playersCount);

        TokenManager.AddTokensToAll(tokensToAdd);

        base.OnTimeExpired();
    }
}