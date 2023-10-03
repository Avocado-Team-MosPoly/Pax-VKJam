using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New RandomItem", menuName = "Random Item", order = 51)]
public class RandomItem : ScriptableObject
{

    [SerializeField] public string SystemName;
    [SerializeField] public int DesignID;
    [SerializeField] protected bool IsEmbedded; 
    protected virtual void Embedded() { IsEmbedded = false; }
    public bool checkEmbedded()
    {
        return IsEmbedded;
    }
    private void Start() => Embedded();
    public virtual void Interact() 
    {
        Catcher_RandomItem.SetData(this);
    }

    public float ResultLesserThan;
}
