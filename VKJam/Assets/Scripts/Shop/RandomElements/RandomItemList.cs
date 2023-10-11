using System.Linq;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New RandomItemList", menuName = "Random Item list", order = 52)]
public class RandomItemList : RandomItem
{
    [SerializeField] private List<RandomItem> Massiv;
    [SerializeField]
    private static int RandomStrike;
    [SerializeField]
    private float RandomStrikeMod;
    protected override void Embedded() { IsEmbedded = true; }
    private void Awake()
    {
        Massiv = Massiv.OrderBy(x => x.ResultLesserThan).ToList();
        //Debug.Log(SystemName);
    }
    override public void Interact()
    {
        float result = Random.Range(1, 100);
        Debug.Log(SystemName + " roll - " + result);
        foreach (var current in Massiv)
        {
            if (current.ResultLesserThan + RandomStrikeMod * RandomStrike > result)
            {
                Debug.Log(current.SystemName);
                    current.Interact();
                break;
            }
        }

    }
}
