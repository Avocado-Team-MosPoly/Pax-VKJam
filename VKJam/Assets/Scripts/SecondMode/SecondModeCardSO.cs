using System.IO;
using System.IO.Compression;
using Unity.Netcode;
using UnityEngine;

public class SecondModeCardSO : BaseCardSO, INetworkSerializable
{
    private byte creatorId;
    private string id;
    private string description;
    private string[] ingredients;
    private Texture2D cardTexture;
    private CardDifficulty difficulty;

    public ulong CreatorId => creatorId;
    public override string Id => id;
    public override string Description => description;
    public override string[] Ingredients => ingredients;
    public override Texture2D CardTexture => cardTexture;
    public override Texture MonsterTexture => null;
    public override Sprite MonsterInBestiarySprite => null;
    public override CardDifficulty Difficulty => difficulty;

    public void Initialize(string id, string description, string[] ingredients, Texture2D cardTexture, CardDifficulty difficulty = CardDifficulty.Dangerous)
    {
        this.id = id;
        this.description = description;
        this.ingredients = ingredients;
        this.cardTexture = cardTexture;
        this.difficulty = difficulty;
    }

    public void AssignCreatorId(ulong clientId)
    {
        creatorId = (byte)clientId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref description);
        serializer.SerializeValue(ref difficulty);

        // Serialize Ingredients

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

        // Serialize CardTexture

        byte[] cardTextureBytes = null;
        if (serializer.IsWriter)
            cardTextureBytes = Compress(cardTexture.GetRawTextureData());

        serializer.SerializeValue(ref cardTextureBytes);

        if (serializer.IsReader)
        {
            cardTexture = new Texture2D(512, 1024)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            cardTexture.LoadRawTextureData(Decompress(cardTextureBytes));
        }
    }

    private static byte[] Compress(byte[] data)
    {
        using MemoryStream output = new MemoryStream();
        using (DeflateStream deflateStream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
        {
            deflateStream.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }
    
    private static byte[] Decompress(byte[] data)
    {
        using MemoryStream input = new MemoryStream(data);
        using DeflateStream deflateStream = new DeflateStream(input, CompressionMode.Decompress);
        using (MemoryStream output = new MemoryStream())
        {
            deflateStream.CopyTo(output);
            return output.ToArray();
        }
    }
}