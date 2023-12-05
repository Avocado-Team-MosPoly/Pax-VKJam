using UnityEngine;
using UnityEngine.UI;

public class SwitchModuleUI : SwitchModule
{
    [SerializeField]
    protected Image SwitchTargetUI;
    public override void SwitchItem(WareData NewItem)
    {
        if (NewItem.Data.Type != Type) return;
        SwitchTargetUI.sprite = NewItem.icon;
    }
}
