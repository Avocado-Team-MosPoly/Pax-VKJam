public class ClientReadiness
{
    public readonly ulong Id;
    public bool IsReady;

    private ClientReadiness() { }

    public ClientReadiness(ulong id, bool isReady)
    {
        Id = id;
        IsReady = isReady;
    }
}