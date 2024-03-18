using UnityEngine;

public class SwitchModule : MonoBehaviour
{
    public ItemType Type;
    [SerializeField] protected DetecterModule SwitchTarget;
    [SerializeField] private SwitchModule Sync;
    [SerializeField] private bool NeedSavePosition;

    private System.Collections.IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);

        if (CustomController.Instance.Custom[(int)Type].Data.productName != "" && CustomController.Instance.Custom[(int)Type].Data.productCode != 0)
            SwitchItem(CustomController.Instance.Custom[(int)Type]);
    }

    public virtual void SwitchItem(WareData NewItem)
    {
        if (NewItem.Data.Type != Type) 
            return;


        //Transform instanceTransform = Instantiate(NewItem.Model, transform).transform;

        if (Instantiate(NewItem.Model, transform).TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            if (SceneLoader.ActiveScene.name != "ProfileCastom")
                return;

            rb.isKinematic = true;
        }

        if (Sync != null) 
        { 
            Sync.Synchronization(NewItem);
        }
    }
    private void Synchronization(WareData NewItem)
    {
        if (!NeedSavePosition)
        {
            Instantiate(NewItem.Model, transform);
        }
        else
        {
            Instantiate(NewItem.Model, SwitchTarget.transform.position, SwitchTarget.transform.rotation, transform);
        }
    }
    public void NewItem(DetecterModule NewItem)
    {
        if (SwitchTarget != null && NewItem != SwitchTarget)
        {
            if (SwitchTarget._Anim != null)
            {
                if (NewItem._Anim == null)
                    NewItem._Anim = NewItem.gameObject.AddComponent<Animator>();
                if (NewItem._Anim.runtimeAnimatorController != SwitchTarget._Anim.runtimeAnimatorController)
                    NewItem._Anim.runtimeAnimatorController = SwitchTarget._Anim.runtimeAnimatorController;
            }
            if (SwitchTarget.Data != null)
                NewItem.Data.Set(SwitchTarget.Data);

            Destroy(SwitchTarget.Object);
        }
        SwitchTarget = NewItem;
    }
}
