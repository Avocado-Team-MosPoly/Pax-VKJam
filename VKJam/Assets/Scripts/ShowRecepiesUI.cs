using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowRecepiesUI : MonoBehaviour
{
    [SerializeField] private GameObject Recept;

    public void SpawnRecept()
    {
        if (Card.activeIngridients.Count>0)
        {
            int rundomNumber = Random.Range(0, Card.activeIngridients.Count - 1);
            Recept.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Card.activeIngridients[rundomNumber];
            Card.activeIngridients.Remove(Card.activeIngridients[rundomNumber]);
            Debug.Log("SpawnRecept");
        }
        else
        {
            Debug.Log("Рецепты кончились");
        }
    }
    public void Hide()
    {
        Recept.SetActive(false);
    }
}
