using UnityEngine;

public class GameCustom : BaseSingleton<GameCustom>
{
    [SerializeField] private SwitchModule[] Custom = new SwitchModule[System.Enum.GetNames(typeof(ItemType)).Length];
    [SerializeField] private CustomController Data;

    private void Start()
    {
        Data = CustomController.Instance;
        Swap();
    }

    private void Swap()
    {
        foreach(var cur in Data.Custom)
        {
            //Logger.Instance.LogError(this, "[Swap] name : " + cur.Data.productName + "; model : " + cur.Model);
            Custom[(int)cur.Data.Type].SwitchItem(cur);
        }
    }
}