using UnityEngine;
[CreateAssetMenu(fileName = "New RandomItem", menuName = "Random Item", order = 51)]
public class RandomItem : ScriptableObject
{

    [SerializeField] public string SystemName;
    [SerializeField] public int DesignID;
    [SerializeField] protected bool IsEmbedded;
    
    public RandomType Type;
    protected virtual void Embedded() { IsEmbedded = false; }
    public bool checkEmbedded()
    {
        return IsEmbedded;
    }
    private void Awake() => Embedded();
    public virtual int Interact() 
    {
        Catcher_RandomItem.SetData(this);
        return DesignID;
    }

    public float ResultLesserThan;
}
