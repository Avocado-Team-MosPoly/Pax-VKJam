using System;
using System.Collections;
using UnityEngine;

public class DonatRouter : MonoBehaviour
{
    public TMPro.TMP_Text AdsCounterTest;
    [SerializeField] private TMPro.TMP_Text buyText;
    [SerializeField] private string buyTextString;

    private void Awake()
    {
        StartCoroutine(DelayRefresh());
    }
    public void BuyCross(int id)
    {
        VK_Connect._executor.RequestBuyTry(id);
        StartCoroutine(DelayRefresh());
    }
    public void Ads()
    {
        VK_Connect._executor.RequestAds();
        StartCoroutine(DelayRefresh());
    }
    private IEnumerator DelayRefresh()
    {
        yield return new WaitForSeconds(1);
        AdsCounterTest.text = "Реклама " + AdManager.GetAdsWatchedToday() + "/3";
        buyText.text = buyTextString;
        CurrencyCatcher._executor.Refresh();
        
    }
    public void BuyTokens(int id)
    {
        Action<string> successRequest = (string response) =>
        {
            CurrencyCatcher._executor.Refresh();
        };
        //Action unsuccessRequest = () => { };

        StartCoroutine(Php_Connect.Request_TokenBuy(id, successRequest, null));
    }
}