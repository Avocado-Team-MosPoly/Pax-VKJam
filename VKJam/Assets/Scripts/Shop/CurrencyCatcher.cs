using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyCatcher : TaskExecutor<CurrencyCatcher>
{
    [SerializeField] private TMPro.TextMeshProUGUI Donat_General;
    [SerializeField] private TMPro.TextMeshProUGUI Donat_Card;
    [SerializeField] private TMPro.TextMeshProUGUI IG;
    [SerializeField] private TMPro.TextMeshProUGUI CP;

    void Awake()
    {
        Denote();
        Refresh();
    }
    public void Refresh()
    {
        Currency Data;
        if (Php_Connect.PHPisOnline) Data = Php_Connect.Request_CurrentCurrency();
        else Data = Php_Connect.Current;
        Donat_General.text = Data.DCurrency.ToString();
        Donat_Card.text = Data.DCurrency.ToString();
        IG.text = Data.IGCurrency.ToString();
        CP.text = Data.CardPiece.ToString();
    }
}
