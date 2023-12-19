using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class VKNameParcer : MonoBehaviour
{
    private TMP_Text nameText;

    private void OnEnable()
    {
        nameText = GetComponent<TMP_Text>();

        nameText.text = Authentication.PlayerName;
    }
}
