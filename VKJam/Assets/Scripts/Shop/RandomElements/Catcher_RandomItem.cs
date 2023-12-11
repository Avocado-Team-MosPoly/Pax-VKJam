using UnityEngine;

public class Catcher_RandomItem : TaskExecutor<Catcher_RandomItem>
{
    [SerializeField] private CurrencyCatcher display;
    [SerializeField] private static RandomItem DroppedItem;
    [SerializeField] private Animator Hand;
    [SerializeField] private GameObject[] WinObjects;
    [SerializeField] private GameObject Current;
    [Header("Display UI")]
    //[SerializeField] private GameObject UI;
    [SerializeField] private Displayer Token;
    [SerializeField] private Displayer Custom;
    [SerializeField] private Displayer Card;
    public static int Result;
    [SerializeField] private PackCardSO Pack;

    private void Awake()
    {
        Denote();
        /*if (NameOutput == null)
        {
            GameObject.FindGameObjectWithTag("Random_Frame").TryGetComponent<TMPro.TMP_Text>(out NameOutput);
        }
        if (Window == null)
        {
            Window = NameOutput.transform.parent.gameObject;
        }
        if (Window != null) Window.SetActive(false);*/
    }
    public static void SetData(RandomItem Data)
    {
        DroppedItem = Data;
        _executor.OnDroppingWhatever();
        Result = Data.DesignID;
    }
    public void Gifter()
    {
        Debug.Log(0);
        if (Php_Connect.PHPisOnline) Php_Connect.Request_Gift(0, Php_Connect.Nickname);
        else Php_Connect
                ._executor
                .RandomBase
                .Interact();
        display.Refresh();
    }
    public void GenerateWin(DesignSelect Sel)
    {
        if (Current != null) Destroy(Current);
        WareData temp = CustomController._executor.Search(Sel);
        Debug.Log(Sel.type + " " + (Sel.type >= 8));
        if (Sel.type >= 8)
        {
            Debug.Log(Mathf.Clamp(WinObjects.Length, 0, 3));
            ActiveWinObject(3);
        }
        else
        {
            Debug.Log("Try create " + temp.Model);
            Current = Instantiate(temp.Model, WinObjects[4].transform);
        }
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
        if (Current != null) Destroy(Current);
        for (int i = 0; i <= Mathf.Clamp(WinObjects.Length, 0, 3);++i)
        {
            if (id == i) WinObjects[i].SetActive(true);
            else WinObjects[i].SetActive(false);
        }

    }
    private void OnDroppingWhatever()
    {
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
                Hand.Play("Wish");
                break;
            default:

                break;
        }
    }

}
