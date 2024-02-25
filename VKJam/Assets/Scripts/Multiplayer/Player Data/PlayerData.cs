using UnityEngine;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>
{
    public byte AvatarIndex;
    public byte AvatarFrameIndex;

    private FixedString64Bytes lobbyPlayerId;
    private FixedString64Bytes name;

    public string LobbyPlayerId
    {
        get
        {
            return lobbyPlayerId.ToString();
        }
        set
        {
            lobbyPlayerId = new(value);

            if (name.Length < value.Length)
                Debug.LogWarning($"[{nameof(PlayerData)}] {nameof(value)} parameter is bigger than {nameof(lobbyPlayerId)} can store");
        }
    }

    public string Name
    {
        get
        {
            return name.ToString();
        }
        set
        {
            name = new(value);

            if (name.Length < value.Length)
                Debug.LogWarning($"[{nameof(PlayerData)}] {nameof(value)} parameter is bigger than {nameof(name)} can store");
        }
    }

    public PlayerData(byte avatarIndex, byte avatarFrameIndex)
    {
        lobbyPlayerId = LobbyManager.Instance.PlayerId;
        name = Authentication.PlayerName;

        AvatarIndex = avatarIndex;
        AvatarFrameIndex = avatarFrameIndex;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref lobbyPlayerId);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref AvatarIndex);
        serializer.SerializeValue(ref AvatarFrameIndex);
    }

    public bool Equals(PlayerData other)
    {
        return this.lobbyPlayerId == other.lobbyPlayerId
            && this.name == other.name
            && this.AvatarIndex == other.AvatarIndex
            && this.AvatarFrameIndex == other.AvatarFrameIndex;
    }

    public override string ToString()
    {
        string result = string.Empty;

        result += "Name: " + name + ", ";
        result += "Avatar Index: " + AvatarIndex + ", ";
        result += "Avatar Frame Index: " + AvatarFrameIndex + ", ";
        result += "Authentication Service Id: " + lobbyPlayerId;

        return result;
    }
}