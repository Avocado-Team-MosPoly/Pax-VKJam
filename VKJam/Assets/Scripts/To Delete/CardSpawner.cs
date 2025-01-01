using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public Transform spawn1;
    public Transform spawn2;
    public Transform spawn3;
    public void SpawnCard(CardSO cardSO1, CardSO cardSO2, CardSO cardSO3)
    {
        if (cardSO1 == null|| cardSO2 == null || cardSO3 == null )
        {
            Debug.LogWarning("Incorrect Card Scriptable Object");
            return;
        }
        spawner(spawn1, cardSO1);
        //CardSO cardInstance = Instantiate(cardPrefab, spawnTransform.position, spawnTransform.rotation, spawnTransform);
    }
    private void spawner(Transform spawner, CardSO cardSO) 
    { 

    }
}
