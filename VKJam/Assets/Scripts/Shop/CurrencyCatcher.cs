using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyCatcher : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI Donat;
    [SerializeField] private TMPro.TextMeshProUGUI IG;
    void Awake()
    {
        Refresh();
    }
    public void Refresh()
    {
        Currency Data = Php_Connect.Request_CurrentCurrency();
        Donat.text = Data.DCurrency.ToString();
        IG.text = Data.IGCurrency.ToString();
    }
}
