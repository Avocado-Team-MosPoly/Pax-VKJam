using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IngredientInfo : MonoBehaviour
{
    public TextMeshProUGUI IngridientName;
    [HideInInspector] public UnityEvent<int> OnGuess;

    private CompareSystem compareSystem;
    private int index;

    public void SetIngridient(string name, int index, CompareSystem compareSystem)
    {
        IngridientName.text = name;
        this.index = index;
        this.compareSystem = compareSystem;
        
        gameObject.SetActive(true);

        GetComponent<Button>().onClick.AddListener(Guess);
    }

    public void Guess()
    {
        if (GameManager.Instance.Stage == Stage.IngredientGuess)
        {
            compareSystem.CompareAnswerServerRpc(index, new ServerRpcParams());
            OnGuess?.Invoke(index);
        }
    }
}
