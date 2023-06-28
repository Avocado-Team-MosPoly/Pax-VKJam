using UnityEngine;

public class Token : MonoBehaviour
{
    private void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        // что происходит при спавне
    }

    public void Destruct()
    {
        // что происходит при уничтожении
        Destroy(gameObject);
    }
}