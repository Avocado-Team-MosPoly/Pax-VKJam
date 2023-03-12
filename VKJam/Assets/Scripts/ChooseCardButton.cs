using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseCardButton : MonoBehaviour
{
    public GameObject CardSpawners;
    public GameObject Button;
    public GameObject ButtonUI;
    public GameObject Text;
    public void UseChooseButton()
    {
        CardSpawners.SetActive(true);
        Button.SetActive(false);
        ButtonUI.SetActive(true);
        Text.SetActive(true);
    }
    public void UseBackButton()
    {
        CardSpawners.SetActive(false);
        Button.SetActive(true);
        ButtonUI.SetActive(false);
        Text.SetActive(false);
    }
}
