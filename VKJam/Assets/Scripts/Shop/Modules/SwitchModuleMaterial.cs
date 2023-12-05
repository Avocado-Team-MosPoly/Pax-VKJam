using UnityEngine;

public class SwitchModuleMaterial : SwitchModule
{

    [SerializeField]
    protected Material SwitchTargetMaterial;
    public override void SwitchItem(WareData NewItem)
    {
        if (NewItem.Data.Type != Type) return;
        SwitchTargetMaterial.SetTexture("_BaseMap", NewItem.icon.texture);
    }
}
