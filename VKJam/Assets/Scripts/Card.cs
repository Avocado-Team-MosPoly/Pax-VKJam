using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public List<GameObject> AllNormalCardPrefab;
    public List<GameObject> AllUebyCardPrefab;
    public List<GameObject> AllNormalSprites;
    public List<GameObject> AllUebySprites;

    public List<GameObject> Spawn;
    private List<GameObject> cardOnScene = new List<GameObject>();
    private List<string> activIngridientList;
    private GameObject activeCardPrefab;
    private List<Sprite> activeIngridients;
    private bool searchForCard;


    //временное решение
    public GameObject CameraBackButton;
    public GameObject CameraButtonAfterChoosingCard;
    
    public void Start()
    {
        NewCard();
    }

    public void NewCard()
    {
        cardOnScene.Clear();
        List<GameObject> localSpawn =  Spawn;
        int cardNumber = Random.Range(0, AllUebyCardPrefab.Count);
        int spawnNumber = Random.Range(0, localSpawn.Count);
        CardInstance cardInstance = Instantiate(AllUebyCardPrefab[cardNumber], localSpawn[spawnNumber].transform.position, localSpawn[spawnNumber].transform.rotation, localSpawn[spawnNumber].transform).GetComponent<CardInstance>();
        localSpawn.RemoveAt(spawnNumber);
        cardInstance.CardSpawner = this;
        cardInstance.Monster = AllUebySprites[cardNumber];
        cardOnScene.Add(cardInstance.gameObject);
        AllUebyCardPrefab.RemoveAt(cardNumber);
        AllUebySprites.RemoveAt(cardNumber);

        for (int i = 0; i < 2; i++)
        {
            cardNumber = Random.Range(0, AllNormalCardPrefab.Count);
            spawnNumber = Random.Range(0, localSpawn.Count);
            cardInstance = Instantiate(AllNormalCardPrefab[cardNumber], localSpawn[spawnNumber].transform.position, localSpawn[spawnNumber].transform.rotation, localSpawn[spawnNumber].transform).GetComponent<CardInstance>();
            localSpawn.RemoveAt(spawnNumber);
            cardInstance.CardSpawner = this;
            cardInstance.Monster = AllNormalSprites[cardNumber];
            cardOnScene.Add(cardInstance.gameObject);
            AllNormalCardPrefab.RemoveAt(cardNumber);
            AllNormalSprites.RemoveAt(cardNumber);
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
                        CameraBackButton.SetActive(false);
                        CameraButtonAfterChoosingCard.SetActive(true);

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
