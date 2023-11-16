using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ChooseFromBook : MonoBehaviour
{
    [HideInInspector] public string GuessedMonster;
    [HideInInspector] public int MonsterId;

    [SerializeField] private CompareSystem compareSystem;
    
    public void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(Guess);
    }
    
    public void Guess()
    {
        compareSystem.CompareAnswerServerRpc(MonsterId, new ServerRpcParams());
    }
}