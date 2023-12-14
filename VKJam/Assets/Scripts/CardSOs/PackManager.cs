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

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);

        //send request whith card packs we have

        //for each pack we own send request which card in pack ownering

        //save prev logic

        string[] resp = Php_Connect.Request_WhichCardInPackOwnering(Active.PackDBIndex).Split('\n');

        foreach (var card in Active.CardInPack) 
        {
            foreach (var element in resp)
            {
                if(card.CardDBIndex.ToString() == element)
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
    }
}
