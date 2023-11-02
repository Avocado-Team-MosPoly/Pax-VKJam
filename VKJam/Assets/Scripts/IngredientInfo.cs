using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class IngredientInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ingridientName;
    [SerializeField] private CompareSystem compareSystem;
    public void SetIngridient(string name)
    {
        ingridientName.text = name;
        gameObject.SetActive(true);

        GetComponent<Button>().onClick.AddListener(
          () => Guess()
      );
    }
    public void Guess()
    {
        if (GameManager.Instance.Stage == Stage.IngredientGuess)
        {
            compareSystem.CompareAnswerServerRpc(ingridientName.text, new ServerRpcParams());
        }
    }
}
