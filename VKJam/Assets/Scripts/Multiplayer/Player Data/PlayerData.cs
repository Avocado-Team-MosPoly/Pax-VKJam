using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;

public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>
{
    public byte AvatarIndex;
    public byte AvatarFrameIndex;

    private FixedString64Bytes authenticationServiceId;
    private FixedString64Bytes name;

    public string AuthenticationServiceId
    {
        get
        {
            return authenticationServiceId.ToString();
        }
        set
        {
            authenticationServiceId = new(value);

            if (name.Length < value.Length)
                Debug.LogWarning($"[{nameof(PlayerData)}] {nameof(value)} parameter is bigger than {nameof(authenticationServiceId)} can store");
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
        authenticationServiceId = AuthenticationService.Instance.PlayerId;
        name = Authentication.PlayerName;

        AvatarIndex = avatarIndex;
        AvatarFrameIndex = avatarFrameIndex;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref authenticationServiceId);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref AvatarIndex);
        serializer.SerializeValue(ref AvatarFrameIndex);
    }

    public bool Equals(PlayerData other)
    {
        return this.authenticationServiceId == other.authenticationServiceId
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
        result += "Authentication Service Id: " + authenticationServiceId;

        return result;
    }
}