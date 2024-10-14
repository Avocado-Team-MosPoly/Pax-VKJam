using UnityEngine;
using TMPro;

public class VKNameParcer : MonoBehaviour
{
    private TMP_Text nameText;

    private void OnEnable()
    {
        nameText = GetComponent<TMP_Text>();

        if(nameText != null)
            nameText.text = UnityServicesAuthentication.PlayerName;
    }
}
