using UnityEngine;
using TMPro;
using System;

public class CurrencyCatcher : TaskExecutor<CurrencyCatcher>
{
    [SerializeField] private TextMeshProUGUI Donat_General;
    [SerializeField] private TextMeshProUGUI Donat_Card;
    [SerializeField] private TextMeshProUGUI IG;
    [SerializeField] private TextMeshProUGUI CP;

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        Action<Currency> onComplete = (Currency currency) =>
        {
            if (Donat_General != null)
                Donat_General.text = "X" + currency.DCurrency.ToString();
            if (Donat_Card != null)
                Donat_Card.text = "X" + currency.DCurrency.ToString();
            if (IG != null)
                IG.text = "X" + currency.IGCurrency.ToString();
            if (CP != null)
                CP.text = "X" + currency.CardPiece.ToString();
        };

        StartCoroutine(Php_Connect.Request_CurrentCurrency(onComplete));
    }
}