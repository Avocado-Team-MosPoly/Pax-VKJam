using UnityEngine;

public class Catcher_RandomItem : MonoBehaviour
{
    [SerializeField] private static TMPro.TMP_Text NameOutput;
    [SerializeField] private static GameObject Window;
    [SerializeField] private CurrencyCatcher display;
    [SerializeField] private static RandomItem DroppedItem;
    [SerializeField] private Animator Hand;
    [SerializeField] private GameObject[] WinObjects;

    public static int Result;

    public delegate void ThisDroppedEvent();
    public static event ThisDroppedEvent OnDropped;

    private void Awake()
    {
        OnDropped += OnDroppingWhatever;
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
        OnDropped?.Invoke();
        Result = Data.DesignID;
        if(Window != null) Window.SetActive(true);
        if (NameOutput != null) NameOutput.text = Data.SystemName;
    }
    public void Gifter()
    {
        Debug.Log(0);
        if (Php_Connect.PHPisOnline) Php_Connect.Request_Gift(0, Php_Connect.Nickname);
        else Php_Connect.randomBase.Interact();
        display.Refresh();
    }

    private void ActiveWinObject(int id)
    {
        for(int i = 0; i < WinObjects.Length;++i)
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
                ActiveWinObject(3);
                Hand.Play("Wish");
                break;
            default:
                
        break;
        }
    }

}
