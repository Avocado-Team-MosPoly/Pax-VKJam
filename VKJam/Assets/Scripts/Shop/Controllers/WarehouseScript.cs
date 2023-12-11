using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class WarehouseScript : TaskExecutor<WarehouseScript>
{
	[SerializeField] private List<Product> ProductInstances = new();

    [SerializeField] private CurrencyCatcher Display;
    [SerializeField]
    private Product Template;
    [SerializeField]
    private Transform WhereInst;
    [SerializeField]
    private CustomController customController;
    [SerializeField, Range(1, 10)]
    private int defaultSection = 1;

    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [SerializeField] private string buyText;
    [SerializeField] private string inOwnText;

    private const int maxInstancesCount = 8;

    private int currentSection = -1;
    private int currentPage = 0;

    private void Awake()
    {
        customController = CustomController._executor;
        Denote();

        SpawnProducts();
        ChangeSection(defaultSection);

        leftButton.onClick.AddListener(PrevPage);
        rightButton.onClick.AddListener(NextPage);
    }

    private void SpawnProducts()
    {
        for (int i = 0; i < maxInstancesCount; i++)
            ProductInstances.Add(Instantiate(Template, WhereInst));
    }

    public void ChangeSection(int section)
    {
        if (section < 1 && section > 10)
            return;

        currentSection = section;
        currentPage = 0;
        int sectionPagesCount = (customController.Categories[currentSection].products.Count + maxInstancesCount - 1) / maxInstancesCount;

        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(currentPage <= sectionPagesCount);

        RefreshView();
    }

    public void NextPage()
    {
        currentPage++;
        int sectionPagesCount = (customController.Categories[currentSection].products.Count + maxInstancesCount - 1) / maxInstancesCount;
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);

        if (currentPage >= sectionPagesCount)
        {
            currentPage = Mathf.Max(0, sectionPagesCount - 1);
        }
        else if (currentPage == sectionPagesCount - 1)
            rightButton.gameObject.SetActive(false);

        RefreshView();
    }

    public void PrevPage()
    {
        currentPage--;
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);

        if (currentPage <= 0)
        {
            currentPage = 0;
            leftButton.gameObject.SetActive(false);
        }

        RefreshView();
    }
    public void RefreshView()
    {
        int startProductIndex = currentPage * maxInstancesCount;

        if (startProductIndex >= customController.Categories[currentSection].products.Count)
        {
            if (maxInstancesCount > customController.Categories[currentSection].products.Count)
                startProductIndex = 0;
        }

        for (int instanceIndex = 0, productIndex = startProductIndex; instanceIndex < maxInstancesCount; instanceIndex++, productIndex++)
        {
            if (productIndex >= customController.Categories[currentSection].products.Count)
            {
                ProductInstances[instanceIndex].gameObject.SetActive(false);
                continue;
            }

            ProductInstances[instanceIndex].ChooseMode = false;
            ProductInstances[instanceIndex].SetData(customController.Categories[currentSection].products[productIndex]);
            if (ProductInstances[instanceIndex].Button)
                ProductInstances[instanceIndex].Button.GetComponentInChildren<TextMeshProUGUI>().text = !ProductInstances[instanceIndex].Data.Data.InOwn || ProductInstances[instanceIndex].ChooseMode ? buyText : inOwnText;
            ProductInstances[instanceIndex].gameObject.SetActive(true);
        }
    }

    public void RemoveProduct(Product productToRemove)
    {
        productToRemove.gameObject.SetActive(false);
        //ProductInstances.Remove(productToRemove);
    }

    public void DestroyProductInstances()
    {
        foreach (var current in ProductInstances)
        {
            Destroy(current.gameObject);
        }

        ProductInstances.Clear();
    }
}