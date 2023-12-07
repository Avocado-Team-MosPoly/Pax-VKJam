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
        else
        {
            if (Php_Connect.Current != null) Data = Php_Connect.Current;
            else  Data = Php_Connect._executor.current;
        }
        Debug.Log(Data);
        if(Donat_General != null) Donat_General.text = Data.DCurrency.ToString();
        if (Donat_Card != null) Donat_Card.text = Data.DCurrency.ToString();
        if (IG != null) IG.text = Data.IGCurrency.ToString();
        if (CP != null) CP.text = Data.CardPiece.ToString();
    }
}
