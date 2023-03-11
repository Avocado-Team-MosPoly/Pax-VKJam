using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public List<GameObject> AllCardPrefab;
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
        for (int i = 0; i < 3; i++)
        {
            int cardNumber = Random.Range(0, AllCardPrefab.Count);
            cardOnScene.Add(Instantiate(AllCardPrefab[cardNumber],  Spawn[i].transform.position, Spawn[i].transform.rotation, Spawn[i].transform));
            AllCardPrefab.RemoveAt(cardNumber);
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
