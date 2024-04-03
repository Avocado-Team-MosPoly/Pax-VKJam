using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button musicOffButton;
    [SerializeField] private Button musicOnButton;

    [Header("Audio")]
    [SerializeField] private Slider audioVolumeSlider;

    private void Start()
    {
        if (BackgroundMusic.Instance != null)
        {
            musicVolumeSlider.value = BackgroundMusic.Instance.Volume;
            musicVolumeSlider.onValueChanged.AddListener(ChangeMusicVolume);

            musicOffButton.onClick.AddListener(MuteMusic);
            musicOnButton.onClick.AddListener(UnmuteMusic);
        }

        audioVolumeSlider.value = SoundList.VolumeObserver.Value;
        audioVolumeSlider.onValueChanged.AddListener(ChangeAudioVolume);
    }

    private void ChangeMusicVolume(float value)
    {
        BackgroundMusic.Instance.Volume = value;
    }

    private void ChangeAudioVolume(float value)
    {
        SoundList.VolumeObserver.Value = value;
    }

    private void MuteMusic()
    {
        musicOffButton.interactable = true;
        musicOnButton.interactable = false;

        SoundList.MuteObserver.Value = true;
        BackgroundMusic.Instance.Mute = true;
    }

    private void UnmuteMusic()
    {
        musicOffButton.interactable = false;
        musicOnButton.interactable = true;

        SoundList.MuteObserver.Value = false;
        BackgroundMusic.Instance.Mute = false;
    }
}