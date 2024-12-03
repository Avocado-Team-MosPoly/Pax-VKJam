public interface IGameManager
{
    // general
    bool IsTeamMode { get; }

    // round
    int CurrentRound { get; }

    // role
    bool IsPainter { get; }
    byte PainterId { get; }
    
    // card
    bool IsDangerousCard { get; }
    string CurrentMonsterName { get; }
    
    // ingredient
    string CurrentIngredientName { get; }
    int IngredientsCount { get; }
}