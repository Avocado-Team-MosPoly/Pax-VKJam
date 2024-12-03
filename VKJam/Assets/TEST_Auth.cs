using UnityEngine;

namespace Pax
{
    public class TEST_Auth : MonoBehaviour
    {
        [SerializeField] private int id;

        private void Start()
        {
            StartCoroutine(Php_Connect.Instance.Init());

            StartCoroutine(Php_Connect.Request_Auth(
                id <= 0 ? Random.Range(0, 10000) : id,
                registered => Debug.Log($"registered (with id - {id}): {registered}"),
                null));
        }
    }
}
