using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetecterModule : MonoBehaviour
{
    public Interactable Data;
    public GameObject Object;
    [HideInInspector] public Animator _Anim;
    private void Awake()
    {
        SwitchModule temp = GetComponentInParent<SwitchModule>();
        _Anim = GetComponent<Animator>();
        if (temp != null) temp.NewItem(this);
    }
}
