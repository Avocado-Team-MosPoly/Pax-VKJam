using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public List<GameObject> AllNormalCardPrefab;
    public List<GameObject> AllUebyCardPrefab;
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
        cardOnScene.Add(Instantiate(AllUebyCardPrefab[cardNumber], Spawn[2].transform.position, Spawn[2].transform.rotation, Spawn[2].transform));
        AllUebyCardPrefab.RemoveAt(cardNumber);

        for (int i = 0; i < 2; i++)
        {
            cardNumber = Random.Range(0, AllNormalCardPrefab.Count);
            cardOnScene.Add(Instantiate(AllNormalCardPrefab[cardNumber],  Spawn[i].transform.position, Spawn[i].transform.rotation, Spawn[i].transform));
            AllNormalCardPrefab.RemoveAt(cardNumber);
            searchForCard = true;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
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
    }

}
