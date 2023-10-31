using Unity.Netcode;

public class TeamIngredientManager : IngredientManager
{
    public TeamIngredientManager(CompareSystem compareSystem) : base(compareSystem) { }

    protected override void CorrectIngredient()
    {
        Log("Correct guess");

        isIngredientGuessed = false;

        int playersCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        TokenManager.AddTokensToAll(playersCount * 2);

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