using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowRecepiesUI : MonoBehaviour
{
    [SerializeField] private GameObject Recept;

    public void SpawnRecept()
    {
        if (Cards.activeIngridients.Count>0)
        {
            int rundomNumber = Random.Range(0, Cards.activeIngridients.Count - 1);
            Recept.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Cards.activeIngridients[rundomNumber];
            Cards.activeIngridients.Remove(Cards.activeIngridients[rundomNumber]);
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
