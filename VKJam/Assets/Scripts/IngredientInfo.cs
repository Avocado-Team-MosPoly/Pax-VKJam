using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class IngredientInfo : MonoBehaviour
{
    public TextMeshProUGUI IngridientName;

    private CompareSystem compareSystem;

    public void SetIngridient(string name, CompareSystem compareSystem)
    {
        IngridientName.text = name;
        this.compareSystem = compareSystem;
        
        gameObject.SetActive(true);

        GetComponent<Button>().onClick.AddListener(Guess);
    }

    public void Guess()
    {
        if (GameManager.Instance.Stage == Stage.IngredientGuess)
        {
            compareSystem.CompareAnswerServerRpc(IngridientName.text, new ServerRpcParams());
        }
    }
}
