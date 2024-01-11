using System.Collections.Generic;
using UnityEngine;

public class SwitchModuleMaterialTexture : SwitchModule
{
    [SerializeField] protected Material SwitchTargetMaterial;
    [SerializeField] private bool FromPrefab = false;

    //[SerializeField] private Renderer Renderer;
    //[SerializeField] private List<int> ChangedMaterialsIndexes;
    
    public override void SwitchItem(WareData NewItem)
    {
        if (NewItem.Data.Type != Type) 
            return;

        Texture2D tempTex = FromPrefab ? NewItem.Model.GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D : NewItem.icon.texture;
        SwitchTargetMaterial.SetTexture("_BaseMap", tempTex);

        /*int counter = 0;
        foreach(var mat in Renderer.materials)
        {
            if (ChangedMaterialsIndexes.Contains(counter))
            {
                mat.SetTexture("_BaseMap", tempTex);
            }

            counter++;
        }*/
    }
}
