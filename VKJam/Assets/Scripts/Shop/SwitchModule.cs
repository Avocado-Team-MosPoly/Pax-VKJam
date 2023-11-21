using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchModule : MonoBehaviour
{
    public ItemType Type;
    [SerializeField] private DetecterModule SwitchTarget;
    public void SwitchItem(WareData NewItem)
    {
        Debug.Log(3);
        Instantiate(NewItem.Model);
        Debug.Log(4);
    }
    public void NewItem(DetecterModule NewItem)
    {
        if (SwitchTarget != null)
        {
            NewItem.Data.Set(SwitchTarget.Data);
            Destroy(SwitchTarget.Object);
        }
        SwitchTarget = NewItem;
    }
}
