using System;
using Unity.Netcode;
using UnityEngine;

public class SecondModeCardInfo : ICard, INetworkSerializable
{
    public ulong CreatorId => creatorId;
    public string Id => id;
    public string Description => description;
    public string[] Ingredients => ingredients;
    public Texture2D CardTexture => cardTexture;
    public CardDifficulty Difficulty => difficulty;

    private byte creatorId;
    private string id;
    private string description;
    private string[] ingredients;
    private Texture2D cardTexture;
    private CardDifficulty difficulty;

    public SecondModeCardInfo(string id, string description, string[] ingredients, Texture2D cardTexture, CardDifficulty difficulty = CardDifficulty.Dangerous)
    {
        this.id = id;
        this.description = description;
        this.ingredients = ingredients;
        this.cardTexture = cardTexture;
        this.difficulty = difficulty;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref description);
        serializer.SerializeValue(ref difficulty);

        #region Serialize Ingredients

        int ingredientsLenght = 0;
        if (serializer.IsWriter)
        {
            ingredientsLenght = ingredients.Length;
        }

        serializer.SerializeValue(ref ingredientsLenght);

        if (serializer.IsReader)
        {
            ingredients = new string[ingredientsLenght];
        }

        for (int i = 0; i < ingredients.Length; i++)
            serializer.SerializeValue(ref ingredients[i]);

        #endregion

        #region Serialize CardTexture

        byte[] cardTextureBytes = cardTexture.GetRawTextureData();
        serializer.SerializeValue(ref cardTextureBytes);

        if (serializer.IsReader)
        {
            cardTexture.LoadRawTextureData(cardTextureBytes);
        }

        #endregion
    }

    public void AssignCreatorId(ulong clientId)
    {
        creatorId = (byte) clientId;
    }
}