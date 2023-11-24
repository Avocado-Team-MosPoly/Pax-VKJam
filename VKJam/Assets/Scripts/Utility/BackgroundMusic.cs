using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private BackgroundMusic instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
}