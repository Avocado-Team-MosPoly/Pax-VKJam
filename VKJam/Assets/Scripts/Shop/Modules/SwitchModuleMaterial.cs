using UnityEngine;

public class SwitchModuleMaterial : SwitchModule
{

    [SerializeField]
    protected Material SwitchTargetMaterial;
    [SerializeField] private bool FromPrefab = false;
    public override void SwitchItem(WareData NewItem)
    {
        if (NewItem.Data.Type != Type) return;
        if(!FromPrefab) SwitchTargetMaterial.SetTexture("_BaseMap", NewItem.icon.texture);
        else SwitchTargetMaterial.SetTexture("_BaseMap", NewItem.icon.texture);
    }
}
