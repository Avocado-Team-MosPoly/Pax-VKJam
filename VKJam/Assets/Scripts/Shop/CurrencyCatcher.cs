using UnityEngine;
using TMPro;

public class CurrencyCatcher : TaskExecutor<CurrencyCatcher>
{
    [SerializeField] private TextMeshProUGUI Donat_General;
    [SerializeField] private TextMeshProUGUI Donat_Card;
    [SerializeField] private TextMeshProUGUI IG;
    [SerializeField] private TextMeshProUGUI CP;

    private void Awake()
    {
        Denote();
        Refresh();
    }
    public void Refresh()
    {
        Currency Data;
        if (Php_Connect.PHPisOnline)
            Data = Php_Connect.Request_CurrentCurrency();
        else
        {
            if (Php_Connect.Current != null)
                Data = Php_Connect.Current;
            else
                Data = Php_Connect._executor.current;
        }

        if(Donat_General != null)
            Donat_General.text = "X" + Data.DCurrency.ToString();
        if (Donat_Card != null)
            Donat_Card.text = Data.DCurrency.ToString();
        if (IG != null)
            IG.text = "X" + Data.IGCurrency.ToString();
        if (CP != null)
            CP.text = Data.CardPiece.ToString();
    }
}