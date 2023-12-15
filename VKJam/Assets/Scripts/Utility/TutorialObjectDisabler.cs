using UnityEngine;

public class TutorialObjectDisabler : MonoBehaviour
{
    [SerializeField] private string uniqueId;

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueId))
        {
            Logger.Instance.LogWarning(this, $"{nameof(uniqueId)} Is null or empty");
            Destroy(this);
            return;
        }

        if (PlayerPrefs.GetInt(uniqueId, 0) == 0)
        {
            PlayerPrefs.SetInt(uniqueId, 1);
        }
        else
        {
            gameObject.SetActive(false);
        }

        Destroy(this);
    }
}