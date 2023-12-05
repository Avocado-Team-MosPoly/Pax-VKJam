using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetecterModule : MonoBehaviour
{
    public Interactable Data;
    public GameObject Object;
    private void Awake()
    {
        GetComponentInParent<SwitchModule>().NewItem(this);
    }
}
