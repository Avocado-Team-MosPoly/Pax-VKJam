using Unity.Netcode;

public class TeamIngredientManager : IngredientManager
{
    public TeamIngredientManager(GameConfigSO config, CompareSystem compareSystem) : base(config, compareSystem) { }

    protected override void CorrectIngredient()
    {
        Log("Correct guess");

        isIngredientGuessed = false;

        int tokensToAdd = config.BonusIngredientGuessed_TM.GetValue(playersCount);
        TokenManager.AddTokensToAll(tokensToAdd);

        OnCorrectIngredient?.Invoke();
    }

    protected override void OnCorrectIngredientGuess(ulong clientId)
    {
        base.OnCorrectIngredientGuess(clientId);
    }

    protected override void OnWrongIngredientGuess(ulong clientId)
    {
        // Empty
    }
}