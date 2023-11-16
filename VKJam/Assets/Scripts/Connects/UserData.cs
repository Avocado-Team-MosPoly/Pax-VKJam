using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviour
{
    public static string UserName;
    public static string UserIMG_URL;
    private void OnEnable()
    {
        if (transform.root == transform)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}