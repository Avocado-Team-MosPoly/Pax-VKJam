using System.Collections.Generic;

public class SecondModeRoundManager : RoundManager
{
    public SecondModeRoundManager(IGameManager gameManager, GameConfigSO gameConfig, IGuessSystem guessSystem, IngredientManager ingredientManager) :
        base(gameManager, gameConfig, guessSystem, ingredientManager) { }

    protected override void LoseRound()
    {
        base.LoseRound();

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

        IReadOnlyList<ulong> correctGuesserAllIds = ingredientManager.CorrectGuesserAllIds;
        Logger.Instance.LogWarning(this, $"{correctGuesserAllIds.Count} player guessed all ingredients");

        correctGuesserIds.Clear();
    }
}