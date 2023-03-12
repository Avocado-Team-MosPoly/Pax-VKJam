using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public List<GameObject> AllNormalCardPrefab;
    public List<GameObject> AllUebyCardPrefab;
    public List<GameObject> AllNormalSprites;
    public List<GameObject> AllUedySprites;

    public List<GameObject> Spawn;
    private List<GameObject> cardOnScene = new List<GameObject>();
    private List<string> activIngridientList;
    private GameObject activeCardPrefab;
    private List<Sprite> activeIngridients;
    private bool searchForCard;
    
    public void Start()
    {
        NewCard();
    }

    public void NewCard()
    {
        cardOnScene.Clear();

        int cardNumber = Random.Range(0, AllUebyCardPrefab.Count);
        CardInstance cardInstance = Instantiate(AllUebyCardPrefab[cardNumber], Spawn[2].transform.position, Spawn[2].transform.rotation, Spawn[2].transform).GetComponent<CardInstance>();
        cardInstance.CardSpawner = this;
        cardInstance.Monster = AllUedySprites[cardNumber];
        cardOnScene.Add(cardInstance.gameObject);
        AllUebyCardPrefab.RemoveAt(cardNumber);

        for (int i = 0; i < 2; i++)
        {
            cardNumber = Random.Range(0, AllNormalCardPrefab.Count);
            cardInstance = Instantiate(AllNormalCardPrefab[cardNumber], Spawn[i].transform.position, Spawn[i].transform.rotation, Spawn[i].transform).GetComponent<CardInstance>();
            cardInstance.CardSpawner = this;
            cardInstance.Monster = AllNormalSprites[cardNumber];
            cardOnScene.Add(cardInstance.gameObject);
            AllNormalCardPrefab.RemoveAt(cardNumber);
            searchForCard = true;
        }
    }

    private void Update()
    {

    }

    public void ChooseIngredients()
    {
        if (searchForCard == true)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    if (cardOnScene.Contains(hit.collider.gameObject))
                    {
                        activIngridientList = hit.collider.gameObject.GetComponent<ScriptOnCard>().Word;
                        activeIngridients = hit.collider.gameObject.GetComponent<ScriptOnCard>().Ingridients;
                        activeCardPrefab = hit.collider.gameObject;
                        searchForCard = false;
                        for (int i = 0; i < cardOnScene.Count; i++)
                        {
                            Destroy(cardOnScene[i]);
                        }
                    }

                }
            }

        }
    }

    public void DisableInteract(GameObject exception)
    {
        for (int i = 0; i < cardOnScene.Count; i++)
        {
            if (cardOnScene[i] != exception)
            {
                cardOnScene[i].GetComponent<Interactable>().ActivityInteractable = false;
            }
        }
    }    
}
