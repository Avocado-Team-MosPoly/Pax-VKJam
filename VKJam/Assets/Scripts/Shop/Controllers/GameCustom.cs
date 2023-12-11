using UnityEngine;
using System.Collections.Generic;

public class GameCustom : TaskExecutor<GameCustom>
{
    [SerializeField] private SwitchModule[] Custom = new SwitchModule[System.Enum.GetNames(typeof(ItemType)).Length];
    [SerializeField]
    private CustomController Data;


    private void Awake()
    {
        Data = CustomController._executor;
        Denote();
        Swap();
    }
    private void Swap()
    {
        foreach(var cur in Data.Custom)
        {
            Logger.Instance.LogError(this, "[Swap] name : " + cur.Data.productName + "; model : " + cur.Model);
            Custom[(int)cur.Data.Type].SwitchItem(cur);
        }
    }
   
  
}