using UnityEngine;

public interface ICard
{
    string Id { get; }
    string Description { get; }

    CardDifficulty Difficulty { get; }
    string[] Ingredients { get; }

    Texture2D CardTexture { get; }
}