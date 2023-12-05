using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    private void Start()
    {
        if (Singleton != this)
            Destroy(gameObject);
    }
}