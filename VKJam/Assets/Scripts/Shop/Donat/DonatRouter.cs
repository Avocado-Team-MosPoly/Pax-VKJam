using System.Collections;
using UnityEngine;

public class DonatRouter : MonoBehaviour
{
    public TMPro.TMP_Text AdsCounterTest;
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
        CurrencyCatcher._executor.Refresh();
        AdsCounterTest.text = "Реклама " + AdManager.GetAdsWatchedToday() + "/3";
    }
    public void BuyTokens(int id)
    {
        Php_Connect.Request_TokenBuy(id);
        CurrencyCatcher._executor.Refresh();
    }
}
