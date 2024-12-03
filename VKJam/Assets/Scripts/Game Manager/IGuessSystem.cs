using UnityEngine.Events;

public interface IGuessSystem
{
    UnityEvent<string, ulong> OnIngredientGuess { get; }
    UnityEvent<string, ulong> OnMonsterGuess { get; }
}