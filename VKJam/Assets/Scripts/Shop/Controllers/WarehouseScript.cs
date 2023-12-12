using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class WarehouseScript : TaskExecutor<WarehouseScript>
{
	private List<Product> productInstances = new();

    [SerializeField] private CurrencyCatcher Display;
    [SerializeField] private Product Template;
    [SerializeField] private Transform WhereInst;
    [SerializeField] private CustomController customController;
    [SerializeField, Range(1, 10)] private int defaultSection = 1;

    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [SerializeField] private string buyText;
    [SerializeField] private string inOwnText;

    [SerializeField] private SoundList soundList;

    private const int maxInstancesCount = 8;

    private int currentSection = -1;
    private int currentPage = 0;

    private void Awake()
    {
        Denote();
    }

    private void Start()
    {
        customController = CustomController._executor;

        SpawnProducts();
        ChangeSection(defaultSection);

        leftButton.onClick.AddListener(PrevPage);
        rightButton.onClick.AddListener(NextPage);
    }

    private void SpawnProducts()
    {
        for (int i = 0; i < maxInstancesCount; i++)
            productInstances.Add(Instantiate(Template, WhereInst));
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
        soundList.Play("Turning the page");
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
        soundList.Play("Turning the page");
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
                Logger.Instance.Log(this, productIndex + " " + customController.Categories[currentSection].products.Count);
                productInstances[instanceIndex].gameObject.SetActive(false);
                continue;
            }

            productInstances[instanceIndex].ChooseMode = false;
            productInstances[instanceIndex].SetData(customController.Categories[currentSection].products[productIndex]);
            if (productInstances[instanceIndex].Button)
                productInstances[instanceIndex].Button.GetComponentInChildren<TextMeshProUGUI>().text = !productInstances[instanceIndex].Data.Data.InOwn || productInstances[instanceIndex].ChooseMode ? buyText : inOwnText;
            productInstances[instanceIndex].gameObject.SetActive(true);
        }
    }

    public void RemoveProduct(Product productToRemove)
    {
        if (productToRemove.Button)
        {
            productToRemove.Button.interactable = false;
            productToRemove.Button.GetComponentInChildren<TextMeshProUGUI>().text = inOwnText;
        }
        //productToRemove.gameObject.SetActive(false);
        //ProductInstances.Remove(productToRemove);
    }

    public void DestroyProductInstances()
    {
        foreach (var current in productInstances)
        {
            Destroy(current.gameObject);
        }

        productInstances.Clear();
    }
}