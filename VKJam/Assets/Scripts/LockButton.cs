using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LockButton : MonoBehaviour
{
    [SerializeField] private int timeToLock;
    private Button btn;
    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(FrozeButton);
    }

    public void FrozeButton()
    {
        +
        StartCoroutine(DisableButtonForSeconds(timeToLock));       
    }
    private IEnumerator DisableButtonForSeconds(float seconds)
    {
        //btn.interactable = false;

        yield return new WaitForSeconds(seconds);

        btn.interactable = true;

    }
}
