using UnityEngine;

public class Catcher_RandomItem : MonoBehaviour
{
    [SerializeField] private static TMPro.TMP_Text NameOutput;
    [SerializeField] private static GameObject Window;
    public static int Result;

    private void Awake()
    {
        if (NameOutput == null)
        {
            NameOutput = GameObject.FindGameObjectWithTag("Random_Frame").GetComponent<TMPro.TMP_Text>();
        }
        if (Window == null)
        {
            Window = NameOutput.transform.parent.gameObject;
        }
        if (Window != null) Window.SetActive(false);
    }
    public static void SetData(RandomItem Data)
    {
        Result = Data.DesignID;
        if(Window != null) Window.SetActive(true);
        if (NameOutput != null) NameOutput.text = Data.SystemName;
    }
    public void Gifter()
    {
        if (Php_Connect.PHPisOnline) Php_Connect.Request_Gift(0, Php_Connect.Nickname);
        else Php_Connect.randomBase.Interact();
    }
}
