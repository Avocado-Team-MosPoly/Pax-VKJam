using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
[CreateAssetMenu()]
public class storeSection : ScriptableObject
{
    public List<WareData> products;
    public ShopFilters sectionType;
    public void Add(WareData target)
    {
        if (products == null)
        {
            products = new List<WareData>();
        }

        products.Add(target);
    }
}
