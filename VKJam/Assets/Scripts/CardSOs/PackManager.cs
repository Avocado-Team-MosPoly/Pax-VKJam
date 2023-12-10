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
        foreach (var current in All)
        {
            //current.PackDataOwnering();
        }
    }
    /*private void OnApplicationQuit()
    {
        foreach (var current in All)
        {
            current.ResetOwning();
        }
    }*/



}
