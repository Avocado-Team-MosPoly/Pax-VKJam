using Unity.Netcode;
using Unity.Services.Authentication;

public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>
{
    public string AuthenticationServiceId;
    public string Name;
    public byte ProfileImageIndex;

    public PlayerData(bool useless = false)
    {
        AuthenticationServiceId = AuthenticationService.Instance.PlayerId;
        Name = Authentication.PlayerName;
        ProfileImageIndex = UserData.UserImageIndex;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref AuthenticationServiceId);
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref ProfileImageIndex);
    }

    public bool Equals(PlayerData other)
    {
        return this.AuthenticationServiceId == other.AuthenticationServiceId
            && this.Name == other.Name
            && this.ProfileImageIndex == other.ProfileImageIndex;
    }

    public override string ToString()
    {
        string result = string.Empty;

        result += "AuthenticationServiceId: " + AuthenticationServiceId + ", ";
        result += "Name: " + Name + ", ";
        result += "ProfileImageIndex: " + ProfileImageIndex;

        return result;
    }
}