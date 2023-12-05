using Unity.Netcode;

public class CustomNetworkManager : NetworkManager
{
    private void Start()
    {
        if (Singleton != null)
        {
            if (Singleton != this)
                Destroy(gameObject);
            else
                Logger.Instance.Log(this, "I'm NM Singleton");
        }
        else
            Logger.Instance.Log(this, "Singleton is null");
    }
}