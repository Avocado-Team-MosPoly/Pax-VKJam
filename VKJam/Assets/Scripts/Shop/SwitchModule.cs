using UnityEngine;

public class SwitchModule : MonoBehaviour
{

    public ItemType Type;
    [SerializeField] private DetecterModule SwitchTarget;
    [SerializeField] private SwitchModule Sync;
    [HideInInspector] public SwitchModule ReverseSync;
    private void Awake()
    {
        if (Sync != null) Sync.ReverseSync = this;
    }
    public void SwitchItem(WareData NewItem)
    {
        if (NewItem.Data.Type != Type) return;
        Instantiate(NewItem.Model, transform);
        if (Sync != null) Sync.SwitchNextNormal(NewItem);
        if (ReverseSync != null) ReverseSync.SwitchNextReverse(NewItem);
    }
    private void Synchronization(WareData NewItem, bool NormalDir)
    {
        Instantiate(NewItem.Model, transform);
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
        if (SwitchTarget != null)
        {
            NewItem.Data.Set(SwitchTarget.Data);
            Destroy(SwitchTarget.Object);
        }
        SwitchTarget = NewItem;
    }
}
