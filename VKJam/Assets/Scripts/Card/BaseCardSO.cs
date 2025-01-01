using UnityEngine;

public abstract class BaseCardSO : ScriptableObject
{
    public abstract string Id { get; }
    public abstract string Description { get; }

    public abstract CardDifficulty Difficulty { get; }
    public abstract string[] Ingredients { get; }

    public abstract Texture2D CardTexture { get; }
    public abstract Texture MonsterTexture { get; }
    public abstract Sprite MonsterInBestiarySprite { get; }
}