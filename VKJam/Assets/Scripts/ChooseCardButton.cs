using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseCardButton : MonoBehaviour
{
    public CardManager CardManager;
    public GameObject Button;
    public GameObject ButtonUI;
    public GameObject Text;

    public void UseChooseButton()
    {
        CardManager.enabled = true;
        Button.SetActive(false);
        //ButtonUI.SetActive(true);
        Text.SetActive(true);
    }

    public void UseBackButton()
    {
        CardManager.enabled = false;
        Button.SetActive(true);
        ButtonUI.SetActive(false);
        Text.SetActive(false);
    }
}
