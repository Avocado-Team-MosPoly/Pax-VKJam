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
    public virtual void RemoveDuplicates()
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
    [ContextMenu("Set image from prefab to Icon")]
    public void ImageTrief()
    {
        foreach (var item in products)
        {
            item.icon = item.Model.GetComponent<UnityEngine.UI.Image>().sprite;
        }
    }
    [ContextMenu("Reset isOwn")]
    public virtual void ResetIsOwn()
    {
        foreach (var product in products)
        {
            product.Data.InOwn = false;
        }
    }
}