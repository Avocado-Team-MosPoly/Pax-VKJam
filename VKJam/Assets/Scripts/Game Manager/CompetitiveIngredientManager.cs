/// <summary> All logic on server </summary>
public class CompetitiveIngredientManager : IngredientManager
{
    public CompetitiveIngredientManager(GameConfigSO config, CompareSystem compareSystem) : base(config, compareSystem) { }

    protected override void CorrectIngredient()
    {
        Log("Correct guess");

        // add tokens
        foreach (byte clientId in correctGuesserIds)
        {
            int tokensToAdd = GameManager.Instance.PainterId != clientId ? config.BonusForIngredient_CM_G.GetValue(playersCount) : config.BonusForIngredient_CM_P.GetValue(playersCount);

            TokenManager.AddTokensToClient(tokensToAdd, clientId);
        }

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

        if (!correctGuesserIds.Contains(GameManager.Instance.PainterId))
            correctGuesserIds.Add(GameManager.Instance.PainterId);
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