public class SecondModeIngredientManager : IngredientManager
{
    public SecondModeIngredientManager(IGameManager gameManager, GameConfigSO config, IGuessSystem guessSystem) :
        base(gameManager, config, guessSystem) { }

    protected override void CorrectIngredient()
    {
        Log("Correct guess");

        isIngredientGuessed = false;
        OnCorrectIngredient?.Invoke();
    }

    protected override void OnCorrectIngredientGuess(ulong clientId)
    {
        base.OnCorrectIngredientGuess(clientId);

        if (!correctGuesserIds.Contains(clientId))
            correctGuesserIds.Add(clientId);

        if (!correctGuesserIds.Contains(gameManager.PainterId))
            correctGuesserIds.Add(gameManager.PainterId);
    }

    protected override void OnWrongIngredientGuess(ulong clientId)
    {
        base.OnWrongIngredientGuess(clientId);

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