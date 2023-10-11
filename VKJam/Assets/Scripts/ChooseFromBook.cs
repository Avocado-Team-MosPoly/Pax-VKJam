using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.VisualScripting;

public class ChooseFromBook : MonoBehaviour
{
    [SerializeField] private CompareSystem compareSystem;
    public string guess;
    public void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(Guess);
    }
    public void Guess()
    {
        compareSystem.CompareAnswerServerRpc(guess, new ServerRpcParams());
    }

}
