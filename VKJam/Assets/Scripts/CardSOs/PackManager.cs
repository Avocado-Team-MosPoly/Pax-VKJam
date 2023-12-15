using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PackManager : MonoBehaviour
{
    /*public delegate void StartLoadEvent(string sceneName);
    public static event StartLoadEvent OnLoad;*/

    public PackCardSO Active;
    public PackCardSO[] All;
    public Dictionary<ulong,List<bool>> PlayersOwnedCard = new Dictionary<ulong, List<bool>>();

    public static PackManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        
    }

    public IEnumerator Init(string cards)
    {
        //send request whith card packs we have

        //for each pack we own send request which card in pack ownering

        //save prev logic

        if (string.IsNullOrEmpty(cards))
        {
            Logger.Instance.LogError(this, new System.FormatException($"{nameof(cards)} is null or empty"));
            yield break;
        }

        string[] resp = cards.Split('\n');

        foreach (var card in Active.CardInPack)
        {
            foreach (var element in resp)
            {
                if (card.CardDBIndex.ToString() == element)
                {
                    card.CardIsInOwn = true;
                    break;
                }
                else
                {
                    card.CardIsInOwn = false;
                }
            }
        }

        Logger.Instance.Log(this, "Initialized");
        yield break;
    }
}
