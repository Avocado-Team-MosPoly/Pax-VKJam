using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button musicOffButton;
    [SerializeField] private Button musicOnButton;

    //[SerializeField] private Slider mouseSensetivitySlider;

    private void Start()
    {
        if (BackgroundMusic.Instance != null)
        {
            musicVolumeSlider.value = BackgroundMusic.Instance.Volume;
            musicVolumeSlider.onValueChanged.AddListener(ChangeVolume);

            musicOffButton.onClick.AddListener(MuteMusic);
            musicOnButton.onClick.AddListener(UnmuteMusic);
        }
    }

    #region Music

    /// <summary> </summary>
    /// <param name="value"> limit - [0, 1] </param>
    private void ChangeVolume(float value)
    {
        BackgroundMusic.Instance.Volume = value;
    }

    private void MuteMusic()
    {
        musicOffButton.interactable = false;
        musicOnButton.interactable = true;

        BackgroundMusic.Instance.Mute = true;
    }

    private void UnmuteMusic()
    {
        musicOffButton.interactable = true;
        musicOnButton.interactable = false;

        BackgroundMusic.Instance.Mute = false;
    }

    #endregion

    //private void ChangeMouseSensetivity(float value)
    //{
        
    //}
}