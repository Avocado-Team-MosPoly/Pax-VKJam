using UnityEngine;

public class SwitchModule : MonoBehaviour
{

    public ItemType Type;
    [SerializeField] protected DetecterModule SwitchTarget;
    [SerializeField] private SwitchModule Sync;
    [HideInInspector] public SwitchModule ReverseSync;
    [SerializeField] private bool NeedSavePosition;
    private System.Collections.IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);
        if (Sync != null)
        {
            Sync.ReverseSync = this;
            Sync.NeedSavePosition = NeedSavePosition;
        }
            if (CustomController._executor.Custom[(int)Type].Data.productName != "" && 
           CustomController._executor.Custom[(int)Type].Data.productCode != 0) 
           SwitchItem(CustomController._executor.Custom[(int)Type]);
    }
    public virtual void SwitchItem(WareData NewItem)
    {
        if (NewItem.Data.Type != Type) return;
        Instantiate(NewItem.Model, transform);
        if (Sync != null) Sync.SwitchNextNormal(NewItem);
        if (ReverseSync != null) ReverseSync.SwitchNextReverse(NewItem);
    }
    private void Synchronization(WareData NewItem, bool NormalDir)
    {
        if(!NeedSavePosition) Instantiate(NewItem.Model, transform);
        else Instantiate(NewItem.Model, SwitchTarget.transform.position, SwitchTarget.transform.rotation, transform);
        if (Sync != null && NormalDir) Sync.SwitchNextNormal(NewItem);
        if (ReverseSync != null && !NormalDir) ReverseSync.SwitchNextReverse(NewItem);
    }
    private void SwitchNextNormal(WareData NewItem)
    {
        if (Sync != null) Sync.Synchronization(NewItem, true);
    }
    private void SwitchNextReverse(WareData NewItem)
    {
        if (ReverseSync != null) ReverseSync.Synchronization(NewItem, false);
    }
    public void NewItem(DetecterModule NewItem)
    {
        if (SwitchTarget != null && NewItem != SwitchTarget)
        {
            if(SwitchTarget._Anim != null)
            {
                if (NewItem._Anim == null)
                {
                    NewItem._Anim = NewItem.gameObject.AddComponent<Animator>();
                }
                    NewItem._Anim.runtimeAnimatorController = SwitchTarget._Anim.runtimeAnimatorController;
            }
            if(SwitchTarget.Data != null) NewItem.Data.Set(SwitchTarget.Data);
            Destroy(SwitchTarget.Object);
        }
        SwitchTarget = NewItem;
    }
}
