using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TranslatePlayersNamberSlider : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI PlayersCoutText;
    [SerializeField] private Slider PlayersCoundSlider;

    public void SetPlayersNumber()
    {
        PlayersCoutText.text = PlayersCoundSlider.value.ToString();
    }
}
