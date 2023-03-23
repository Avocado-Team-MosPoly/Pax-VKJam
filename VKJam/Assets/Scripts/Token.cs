using UnityEngine;

public class Token : MonoBehaviour
{
    public void OnSpawn()
    {
        // что происходит при спавне
    }

    public void OnDestruct()
    {
        // что происходит при уничтожении
        Destroy(gameObject);
    }
}