using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowRecepiesUI : MonoBehaviour
{
    public GameObject[] Recepies;

    public void SpawnRecepies()
    {
        Hide();
        int cardNumber = Random.Range(0, Recepies.Length);
        Recepies[cardNumber].SetActive(true);
    }
    public void Hide()
    {
        foreach (GameObject hide in Recepies)
        {
            hide.SetActive(false);
        }
    }

}
