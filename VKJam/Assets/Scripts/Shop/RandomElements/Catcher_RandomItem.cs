using Tiractor.Sound;
using UnityEngine;

public class Catcher_RandomItem : BaseSingleton<Catcher_RandomItem>
{
    public static int Result;

    [SerializeField] private CurrencyCatcher display;
    [SerializeField] private Animator Hand;
    [SerializeField] private GameObject[] WinObjects;

    [Header("Display UI")]
    //[SerializeField] private GameObject UI;
    [SerializeField] private Displayer Token;
    [SerializeField] private Displayer Custom;
    [SerializeField] private Displayer Card;

    [SerializeField] private SoundList soundList;

    private static RandomItem DroppedItem;

    public static void SetData(RandomItem Data)
    {
        DroppedItem = Data;
        Instance.OnDroppingWhatever();
        Result = Data.DesignID;
    }

    public void Gifter()
    {
        if (Php_Connect.PHPisOnline)
        {
            StartCoroutine(Php_Connect.Request_Gift(0, Php_Connect.Nickname));
        }
    }

    public void GenerateWin(DesignSelect Sel)
    {
        WareData temp = CustomController.Instance.Search(Sel);
        Debug.Log(Sel.type + " " + (Sel.type >= 8));

        ActiveWinObject(2);

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
            Card.SetData(PackManager.Instance.Active.SearchCardSystemById(data));
        }
    }
    private void ActiveWinObject(int id)
    {
        for (int i = 0; i < WinObjects.Length; i++)
        {
            if (id == i) 
                WinObjects[i].SetActive(true);
            else 
                WinObjects[i].SetActive(false);
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
            case RandomType.Custom:
                Hand.Play("Wish");
                break;
            default:

                break;
        }
    }

    public void DropTokens(int tokenAmount) 
    {
        ActiveWinObject(0);
        Hand.Play("GiveItem");
        UIWin(RandomType.Token, tokenAmount);
    }
    public void CallFiga()
    {
        ActiveWinObject(-1);
        Hand.Play("Figa");
    }

    public void PlaySound()
    {
        soundList.Play("water-splash");
    }
}