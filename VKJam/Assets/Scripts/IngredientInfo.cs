using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class IngredientInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ingridientName;
    [SerializeField] private GameObject ingredientImage;
    [SerializeField] private CompareSystem compareSystem;
    public void SetIngridient(string name, Sprite picture)
    {
        ingridientName.text = name;
        ingredientImage.GetComponent<Image>().sprite = picture;

//        GetComponent<Button>().onClick.AddListener(
//           () => 
//       );
    }
    public void Guess()
    {
        compareSystem.CompareAnswerServerRpc(ingridientName.text, new ServerRpcParams());
    }
}
