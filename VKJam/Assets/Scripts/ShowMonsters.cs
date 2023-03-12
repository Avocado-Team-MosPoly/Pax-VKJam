using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMonsters : MonoBehaviour
{
    public GameObject Sprite;

    void Show()
    {
        Monster.SetActive(true);
    }

}
