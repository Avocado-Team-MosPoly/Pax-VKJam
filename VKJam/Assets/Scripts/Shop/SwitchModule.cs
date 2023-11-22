using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchModule : MonoBehaviour
{
    public ItemType Type;
    [SerializeField] private DetecterModule SwitchTarget;
    public void SwitchItem(WareData NewItem)
    {
        Instantiate(NewItem.Model, transform);
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
