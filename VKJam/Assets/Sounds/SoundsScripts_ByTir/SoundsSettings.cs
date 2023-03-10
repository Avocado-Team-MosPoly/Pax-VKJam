using UnityEngine;
using UnityEngine.UI;
public class SoundsSettings : MonoBehaviour
{
    [Header("Audio settings")]
    [Range(0, 1)] [SerializeField] private static float Master = 1;
    [Range(0, 1)] [SerializeField] private static float Audio = 1;
    [Range(0, 1)] [SerializeField] private static float Music = 1;
    public delegate void VolumeChanging();
    public static event VolumeChanging _VolumeChanging;
    public static float GetMaster() { return Master; }
    public static float GetAudio() { return Audio; }
    public static float GetMusic() { return Music; }
    public void CorrectingMaster(Slider ValueFrom)
    {
        Master = Mathf.Clamp01(ValueFrom.value);
        _VolumeChanging();
    }
    public void CorrectingMaster(float Value)
    {
        Master = Mathf.Clamp01(Value);
        _VolumeChanging();
    }
    public void CorrectingAudio(Slider ValueFrom)
    {
        Audio = Mathf.Clamp01(ValueFrom.value);
        _VolumeChanging();
    }
    public void CorrectingAudio(float Value)
    {
        Audio = Mathf.Clamp01(Value);
        _VolumeChanging();
    }
    public void CorrectingMusic(Slider ValueFrom)
    {
        Music = Mathf.Clamp01(ValueFrom.value);
        _VolumeChanging();
    }
    public void CorrectingMusic(float Value)
    {
        Music = Mathf.Clamp01(Value);
        _VolumeChanging();
    }
    private void Awake()
    {
        if (PlayerPrefs.HasKey("MasterVolume")) Master = PlayerPrefs.GetFloat("MasterVolume");
        if (PlayerPrefs.HasKey("AudioVolume")) Audio = PlayerPrefs.GetFloat("AudioVolume");
        if (PlayerPrefs.HasKey("MusicVolume")) Music = PlayerPrefs.GetFloat("MusicVolume");
    }
    public void Save()
    {
        PlayerPrefs.SetFloat("MasterVolume", Master);
        PlayerPrefs.SetFloat("AudioVolume", Audio);
        PlayerPrefs.SetFloat("MusicVolume", Music);
        PlayerPrefs.Save();
    }
}