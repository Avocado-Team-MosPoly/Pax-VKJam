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
            if (GameManager.Instance.PainterId != clientId)
            {
                int tokensToAdd = config.BonusForIngredient_CM_G.GetValue(playersCount);
                TokenManager.AddTokensToClient(tokensToAdd, clientId);
            }
            else
            {
                int tokensToAdd = config.BonusForIngredient_CM_P.GetValue(playersCount);
                TokenManager.AddTokensToClient(tokensToAdd, clientId);
            }
        }

        // calculate player without mistakes
        foreach (ulong clientId in correctGuesserAllIds.Keys)
        {
            if (!correctGuesserIds.Contains(clientId))
                correctGuesserAllIds[clientId] = false;
        }

        correctGuesserIds.Clear();
        isIngredientGuessed = false;
        OnCorrectIngredient?.Invoke();
    }

    protected override void OnCorrectIngredientGuess(ulong clientId)
    {
        base.OnCorrectIngredientGuess(clientId);

        correctGuesserIds.Add(clientId);
        if (!correctGuesserIds.Contains(GameManager.Instance.PainterId))
            correctGuesserIds.Add(GameManager.Instance.PainterId);
    }

    protected override void OnWrongIngredientGuess(ulong clientId)
    {
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