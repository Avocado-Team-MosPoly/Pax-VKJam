using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class WarehouseScript : BaseSingleton<WarehouseScript>
{
	private List<Product> productInstances = new();

    [SerializeField] private CurrencyCatcher Display;
    [SerializeField] private Product Template;
    [SerializeField] private Transform WhereInst;
    [SerializeField, Range(1, 10)] private int defaultSection = 1;

    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [SerializeField] private string buyText;
    [SerializeField] private string inOwnText;

    [SerializeField] private SoundList soundList;

    private const int maxInstancesCount = 8;

    private int currentSection = -1;
    private int currentPage = 0;

    private void Start()
    {
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
        int sectionPagesCount = (CustomController.Instance.Categories[currentSection].products.Count + maxInstancesCount - 1) / maxInstancesCount;

        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(currentPage < sectionPagesCount - 1);

        RefreshView();
    }

    public void NextPage()
    {
        currentPage++;
        int sectionPagesCount = (CustomController.Instance.Categories[currentSection].products.Count + maxInstancesCount - 1) / maxInstancesCount;
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

        if (startProductIndex >= CustomController.Instance.Categories[currentSection].products.Count)
        {
            if (maxInstancesCount > CustomController.Instance.Categories[currentSection].products.Count)
                startProductIndex = 0;
        }

        for (int instanceIndex = 0, productIndex = startProductIndex; instanceIndex < maxInstancesCount; instanceIndex++, productIndex++)
        {
            if (productIndex >= CustomController.Instance.Categories[currentSection].products.Count)
            {
                //Logger.Instance.Log(this, productIndex + " " + CustomController.Executor.Categories[currentSection].products.Count);
                productInstances[instanceIndex].gameObject.SetActive(false);
                continue;
            }
            if (CustomController.Instance.Categories[currentSection].products[productIndex].Data.productCode == 0)
            {
                //Logger.Instance.Log(this, CustomController.Executor.Categories[currentSection].products[productIndex].Data.productName);
                instanceIndex--;
                continue;
            }

            UnityAction onClick = () => BackgroundMusic.Instance.GetComponentInChildren<SoundList>().Play("button-click");

            productInstances[instanceIndex].ChooseMode = false;
            productInstances[instanceIndex].SetData(CustomController.Instance.Categories[currentSection].products[productIndex]);
            if (productInstances[instanceIndex].Button)
            {
                if (!productInstances[instanceIndex].Data.Data.InOwn || productInstances[instanceIndex].ChooseMode)
                {
                    productInstances[instanceIndex].Button.onClick.AddListener(onClick);
                    productInstances[instanceIndex].Button.GetComponentInChildren<TextMeshProUGUI>().text = buyText;
                }
                else
                    productInstances[instanceIndex].Button.GetComponentInChildren<TextMeshProUGUI>().text = inOwnText;
            }
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