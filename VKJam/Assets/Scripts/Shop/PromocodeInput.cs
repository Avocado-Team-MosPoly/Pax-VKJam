using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class PromocodeInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField promocode;
    [SerializeField] private GameObject button;
    private void Start()
    {
        promocode.onSubmit.AddListener(OnSubmit);
    }
    private void OnSubmit(string promo)
    {
        Php_Connect.Instance.StartCoroutine(Php_Connect.Request_ActivatePromocod(promo, OnSuccess, OnUnsuccess));
        gameObject.SetActive(false);
        promocode.SetTextWithoutNotify("");
    }

    private void OnSuccess()
    {
        Php_Connect.Instance.StartCoroutine(Php_Connect.Request_CurrentCurrency(null));
        Catcher_RandomItem.Instance.DropTokens(50);
        gameObject.SetActive(false);
        button.SetActive(true);
    }

    private void OnUnsuccess()
    {
        Catcher_RandomItem.Instance.CallFiga();
        button.SetActive(true);
    }
}
