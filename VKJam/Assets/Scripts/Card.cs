using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public List<GameObject> AllCardPrefab;
    public List<GameObject> Spawn;
    private List<GameObject> cardOnScene = new List<GameObject>();
    private string activeWord;
    private GameObject activeCardPrefab;
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
            cardOnScene.Add(Instantiate(AllCardPrefab[cardNumber], Spawn[i].transform.position, Quaternion.identity));
            AllCardPrefab.RemoveAt(cardNumber);
            searchForCard = true;
        }   
    }
    void Update()
    {
        if (searchForCard==true)
        {
            if ((Input.touchCount > 0) && (Input.touches[0].phase == TouchPhase.Began))
            {
                Debug.Log("q");
                Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        Debug.Log("a");
                        if (cardOnScene.Contains(hit.collider.gameObject))
                        {
                            Debug.Log("r");
                            activeWord = hit.collider.gameObject.GetComponent<ScriptOnCard>().Word;
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
