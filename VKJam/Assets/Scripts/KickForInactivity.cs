using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickForInactivity : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine("Kick");
    }
    IEnumerator Kick()
    {
        yield return new WaitForSeconds(20);
        Debug.LogError("Hi");
    }
}
