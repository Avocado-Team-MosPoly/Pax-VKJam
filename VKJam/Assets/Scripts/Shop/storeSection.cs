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
    [ContextMenu("Remove Duplicates")]
    public void RemoveDuplicates()
    {
        var existingModels = new HashSet<GameObject>();
        var uniqueProducts = new List<WareData>();

        foreach (var product in products)
        {
            if (!existingModels.Contains(product.Model))
            {
                uniqueProducts.Add(product);
                existingModels.Add(product.Model); 
            }
        }

        products = uniqueProducts;
    }
}
