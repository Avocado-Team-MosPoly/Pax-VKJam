using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LockButton : MonoBehaviour
{
    [SerializeField] private float timeToLock = 1.5f;
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(FreezeButton);
    }

    public void FreezeButton()
    {
        StartCoroutine(DisableButtonForSeconds(timeToLock));
    }

    private IEnumerator DisableButtonForSeconds(float seconds)
    {
        btn.interactable = false;

        yield return new WaitForSeconds(seconds);

        btn.interactable = true;

    }
}