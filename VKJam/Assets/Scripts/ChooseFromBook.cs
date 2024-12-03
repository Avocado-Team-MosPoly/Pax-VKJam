using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ChooseFromBook : MonoBehaviour
{
    [HideInInspector] public string GuessedMonster;
    [HideInInspector] public int MonsterId;
    [HideInInspector] public bool Selected;
    [HideInInspector] public Action<int> OnSelected;

    [SerializeField] private FirstModeGuessSystem compareSystem;
    [SerializeField] private GameObject checkTic;
    
    public void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(Guess);
    }
    
    public void Guess()
    {
        compareSystem.SendAnswerServerRpc(MonsterId, new ServerRpcParams());
        OnSelected?.Invoke(MonsterId);
        Selected = true;
        Show(Selected);
    }

    public void Show(bool status)
    {
        if (!Selected)
        {
            checkTic.SetActive(status);
        }
        else
        {
            checkTic.SetActive(true);
        }
    }
}