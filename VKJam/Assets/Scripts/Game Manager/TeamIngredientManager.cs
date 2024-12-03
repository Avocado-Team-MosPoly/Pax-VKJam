/// <summary> All logic on server </summary>
public class TeamIngredientManager : IngredientManager
{
    public TeamIngredientManager(IGameManager gameManager, GameConfigSO config, IGuessSystem guessSystem) :
        base(gameManager, config, guessSystem) { }

    protected override void CorrectIngredient()
    {
        Log("Correct guess");

        isIngredientGuessed = false;

        int tokensToAdd = config.BonusIngredientGuessed_TM.GetValue(playersCount) * playersCount;
        TokenManager.AddTokensToAll(tokensToAdd);

        isIngredientGuessed = false;
        OnCorrectIngredient?.Invoke();
    }

    protected override void OnCorrectIngredientGuess(ulong clientId)
    {
        base.OnCorrectIngredientGuess(clientId);

        if (!correctGuesserIds.Contains(clientId))
        {
            correctGuesserIds.Add(clientId);
        }
        
        if (!correctGuesserIds.Contains(gameManager.PainterId))
            correctGuesserIds.Add(gameManager.PainterId);
    }
}