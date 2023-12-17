using UnityEngine;

public class Catcher_RandomItem : TaskExecutor<Catcher_RandomItem>
{
    [SerializeField] private CurrencyCatcher display;
    private static RandomItem DroppedItem;
    [SerializeField] private Animator Hand;
    [SerializeField] private GameObject[] WinObjects;
    [Header("Display UI")]
    //[SerializeField] private GameObject UI;
    [SerializeField] private Displayer Token;
    [SerializeField] private Displayer Custom;
    [SerializeField] private Displayer Card;
    public static int Result;
    [SerializeField] private PackCardSO Pack;

    public static void SetData(RandomItem Data)
    {
        DroppedItem = Data;
        Executor.OnDroppingWhatever();
        Result = Data.DesignID;
    }
    public void Gifter()
    {
        if (Php_Connect.PHPisOnline)
            StartCoroutine(Php_Connect.Request_Gift(0, Php_Connect.Nickname));
        else
            Php_Connect.RandomBase.Interact();

        display.Refresh();
    }
    public void GenerateWin(DesignSelect Sel)
    {
        WareData temp = CustomController.Executor.Search(Sel);
        Debug.Log(Sel.type + " " + (Sel.type >= 8));

        ActiveWinObject(3);

        UIWin(temp);
    }
    public void UIWin(WareData data)
    {
        //UI.SetActive(true);
        Custom.SetData(data);
        Custom.gameObject.SetActive(true);
    }

    public void UIWin(RandomType Type, int data)
    {
        //UI.SetActive(true);
        if (Type == RandomType.Token)
        {
            Token.gameObject.SetActive(true);
            Token.SetData(data);
        }
        else if (Type == RandomType.Card)
        {
            Card.gameObject.SetActive(true);
            Card.SetData(Pack.SearchCardSystemById(data));
        }
    }
    private void ActiveWinObject(int id)
    {
        for (int i = 0; i <= Mathf.Clamp(WinObjects.Length, 0, 3);++i)
        {
            if (id == i) WinObjects[i].SetActive(true);
            else WinObjects[i].SetActive(false);
        }

    }
    private void OnDroppingWhatever()
    {
        Debug.LogWarning("Type = " + DroppedItem.Type);
        switch (DroppedItem.Type)
        {
            case RandomType.Nothing:
                ActiveWinObject(-1);
                Hand.Play("WishFiga");
                break;
            case RandomType.Token:
                ActiveWinObject(0);
                Hand.Play("Wish");
                break;
            case RandomType.Card:
                ActiveWinObject(1);
                Hand.Play("Wish");
                break;
            case RandomType.CardPiece:
                ActiveWinObject(2);
                Hand.Play("Wish");
                break;
            case RandomType.Custom:
                ActiveWinObject(2);
                Hand.Play("Wish");
                break;
            default:

                break;
        }
    }

}
