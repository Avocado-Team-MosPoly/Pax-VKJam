using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SwitchModuleMaterial : SwitchModule
{
    private Renderer _renderer;
    private Renderer Renderer => _renderer ??= GetComponent<Renderer>();

    public override void SwitchItem(WareData NewItem)
    {
        if (NewItem.Data.Type != Type) 
            return;

        if (NewItem.Model.TryGetComponent(out Renderer targetRenderer))
            Renderer.material = targetRenderer.sharedMaterial;
        else
            Logger.Instance.LogWarning(this, "New data model doesn't have 'Renderer' component");
    }
}