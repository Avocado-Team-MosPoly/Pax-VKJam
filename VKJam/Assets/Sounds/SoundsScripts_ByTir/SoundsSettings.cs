using UnityEngine;
using UnityEngine.UI;

public class SoundsSettings : MonoBehaviour
{
    [Header("Audio settings")]
    [Range(0, 1)] [SerializeField] private static float Master = 1;
    [Range(0, 1)] [SerializeField] private static float Audio = 1;
    [Range(0, 1)] [SerializeField] private static float Music = 1;
    [SerializeField] private Slider MasterSlider;
    [SerializeField] private Slider AudioSlider;
    [SerializeField] private Slider MusicSlider;
    public delegate void VolumeChanging();
    public static event VolumeChanging _VolumeChanging;
    public static float GetMaster() { return Master; }
    public static float GetAudio() { return Audio; }
    public static float GetMusic() { return Music; }
    public void CorrectingMaster(Slider ValueFrom)
    {
        Master = Mathf.Clamp01(ValueFrom.value);
        _VolumeChanging?.Invoke();
    }
    public void CorrectingMaster(float Value)
    {
        Master = Mathf.Clamp01(Value);
        _VolumeChanging?.Invoke();
    }
    public void CorrectingAudio(Slider ValueFrom)
    {
        Audio = Mathf.Clamp01(ValueFrom.value);
        _VolumeChanging?.Invoke();
    }
    public void CorrectingAudio(float Value)
    {
        Audio = Mathf.Clamp01(Value);
        _VolumeChanging?.Invoke();
    }
    public void CorrectingMusic(Slider ValueFrom)
    {
        Music = Mathf.Clamp01(ValueFrom.value);
        _VolumeChanging?.Invoke();
    }
    public void CorrectingMusic(float Value)
    {
        Music = Mathf.Clamp01(Value);
        _VolumeChanging?.Invoke();
    }
    private void SetSettings(Slider Target, float value)
    {
        Target.maxValue = 1;
        Target.minValue = 0;
        Target.value = value;
    }

    private void Awake()
    {
        if (PlayerPrefs.HasKey("MasterVolume")) Master = PlayerPrefs.GetFloat("MasterVolume");
        if (PlayerPrefs.HasKey("AudioVolume")) Audio = PlayerPrefs.GetFloat("AudioVolume");
        if (PlayerPrefs.HasKey("MusicVolume")) Music = PlayerPrefs.GetFloat("MusicVolume");
        if (MasterSlider != null)
        {
            MasterSlider.onValueChanged.RemoveAllListeners();
            SetSettings(MasterSlider, Master);
            MasterSlider.onValueChanged.AddListener((value) => CorrectingMaster(MasterSlider));
        }
        if (AudioSlider != null)
        {
            AudioSlider.onValueChanged.RemoveAllListeners();
            SetSettings(AudioSlider, Audio);
            AudioSlider.onValueChanged.AddListener((value) => CorrectingAudio(AudioSlider));
        }
        if (MusicSlider != null)
        {
            MusicSlider.onValueChanged.RemoveAllListeners();
            SetSettings(MusicSlider, Music);
            MusicSlider.onValueChanged.AddListener((value) => CorrectingMusic(MusicSlider));
        }
        _VolumeChanging?.Invoke();
    }
    public void Save()
    {
        PlayerPrefs.SetFloat("MasterVolume", Master);
        PlayerPrefs.SetFloat("AudioVolume", Audio);
        PlayerPrefs.SetFloat("MusicVolume", Music);
        PlayerPrefs.Save();
    }
}