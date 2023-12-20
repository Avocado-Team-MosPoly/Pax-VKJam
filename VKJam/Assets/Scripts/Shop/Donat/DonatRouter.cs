using System;
using System.Collections;
using UnityEngine;

public class DonatRouter : TaskExecutor<DonatRouter>
{
    public TMPro.TMP_Text AdsCounterTest;
    [SerializeField] private TMPro.TMP_Text buyText;
    [SerializeField] private string buyTextString;

    private void Start()
    {
        StartCoroutine(DelayRefresh());
    }
    public void BuyCross(int id)
    {
        VK_Connect.Executor.RequestBuyTry(id);
        StartCoroutine(DelayRefresh());
    }
    public void Ads()
    {
        VK_Connect.Executor.RequestAds();
    }
    public IEnumerator DelayRefresh()
    {
        yield return new WaitForSeconds(1);
        AdsCounterTest.text = "Реклама " + AdManager.GetAdsWatchedToday() + "/3";
        buyText.text = buyTextString;
        CurrencyCatcher.Executor.Refresh();
        
    }
    public void BuyTokens(int id)
    {
        Action<string> successRequest = (string response) =>
        {
            CurrencyCatcher.Executor.Refresh();
        };
        //Action unsuccessRequest = () => { };

        StartCoroutine(Php_Connect.Request_TokenBuy(id, successRequest, null));
    }
}