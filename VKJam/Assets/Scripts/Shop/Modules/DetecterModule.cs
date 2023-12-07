using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetecterModule : MonoBehaviour
{
    public Interactable Data;
    public GameObject Object;
    private void Awake()
    {
        SwitchModule temp = GetComponentInParent<SwitchModule>();
        if(temp != null) temp.NewItem(this);

    }
}
