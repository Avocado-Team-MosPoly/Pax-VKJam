using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cards : MonoBehaviour
{
    [SerializeField] private List<GameObject> AllNormalCardPrefab;
    [SerializeField] private List<GameObject> AllUebyCardPrefab;
    [SerializeField] private List<GameObject> AllNormalSprites;
    [SerializeField] private List<GameObject> AllUebySprites;

    [SerializeField] private List<GameObject> Spawn;
    private List<GameObject> cardOnScene = new List<GameObject>();
    private GameObject activeCardPrefab;
    static public List<string> activeIngridients;
    static public string activeMonsterName;
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
                        activeIngridients = hit.collider.gameObject.GetComponent<Card>().CardSO.Ingredients.ToList();
                        Debug.Log("word on card = " + hit.collider.gameObject);
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
