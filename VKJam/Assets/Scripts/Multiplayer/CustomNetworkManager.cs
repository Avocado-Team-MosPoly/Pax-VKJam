using Unity.Netcode;

public class CustomNetworkManager : NetworkManager
{
    public void Start()
    {
        if (Singleton != null)
        {
            if (Singleton != this)
            {
                //Logger.Instance.Log(this, "I'm not the NM Singleton. I'm destroyed");
                Destroy(gameObject);
            }
            //else
            //    Logger.Instance.Log(this, "I'm NM Singleton");
        }
        //else
        //    Logger.Instance.Log(this, "Singleton is null");
    }
}