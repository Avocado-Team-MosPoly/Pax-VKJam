using System.Collections;
using UnityEngine;

namespace Pax
{
    public class TEST_Auth : MonoBehaviour
    {
        [SerializeField] private int id;
        [SerializeField] private string promo = "TEST2025";

        [ContextMenu(nameof(Authenticate))]
        public IEnumerator Authenticate()
        {
            yield return StartCoroutine(Php_Connect.Instance.Init());

            yield return StartCoroutine(Php_Connect.Request_Auth(
                id <= 0 ? Random.Range(0, 10000) : id,
                registered => Debug.Log($"registered (with id - {id}): {registered}"),
                null));
        }

        [ContextMenu(nameof(ActivatePromo))]
        public void ActivatePromo()
        {
            StartCoroutine(Php_Connect.Request_ActivatePromocod(promo, () => Debug.Log("Success"), () => Debug.LogWarning("Unsuccess")));
        }
    }
}
