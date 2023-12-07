using System.Collections;
using UnityEngine;

public class DonatRouter : MonoBehaviour
{
    public TMPro.TMP_Text AdsCounterTest;
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
        CurrencyCatcher._executor.Refresh();
        
    }
    public void BuyTokens(int id)
    {
        Php_Connect.Request_TokenBuy(id);
        CurrencyCatcher._executor.Refresh();
    }
}
