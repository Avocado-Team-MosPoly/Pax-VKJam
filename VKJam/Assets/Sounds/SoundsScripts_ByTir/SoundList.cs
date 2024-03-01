using System;
using UnityEngine;
using System.Collections;

[Serializable]
public struct Audio
{
    [Tooltip("This used for detecting this elements for Play function")]
    public string KeyName;
    public AudioClip[] Sounds;
}

[RequireComponent(typeof(AudioSource))]
[Serializable]
public class SoundList : MonoBehaviour
{
    [Header("Audio settings")]
    [Tooltip("This created for correct using Settings")] public bool IsMusic;
    [Tooltip("While true new sound will not playing if old still playing")] [SerializeField] private bool WaitingSoundEnd = true;

    [Header("Audio list")]
    [SerializeField] private Audio[] Everything;
    [SerializeField] private bool useSettings = true;
    private AudioSource _source;

    private bool NowPlaying;

    public static VariableObserver<float> VolumeObserver { get; private set; } = new(1f);
    public static VariableObserver<bool> MuteObserver { get; private set; } = new(false);

    public const string KEY_VOLUME = "Audio_Volume";

    private void Awake()
    {
        NowPlaying = false;
        _source = GetComponent<AudioSource>(); // Saving standart output Source for more quickly 
        //ChangeVolume(); // For get pre-setting
        //SoundsSettings._VolumeChanging += ChangeVolume; // Now SoundSettings can change Volume, when changes Value in him
        //_source.volume = VolumeObserver.Value;
        
        if (useSettings)
        {
            // static observer
            VolumeObserver.ValueChanged += ChangeVolume;
        }

        // static observer
        VolumeObserver.Value = PlayerPrefs.GetFloat(KEY_VOLUME, 0.5f);
        
        _source.mute = MuteObserver.Value;
        MuteObserver.ValueChanged += Mute;
    }

    private void OnDestroy()
    {
        if (useSettings)
            VolumeObserver.ValueChanged -= ChangeVolume;

        MuteObserver.ValueChanged -= Mute;

    }

    public void Mute(bool value)
    {
        _source.mute = value;
    }

    public void ChangeVolume(float volume)
    {
        // Cause I use static var in Settings I can interact with base class
        //_source.volume = SoundsSettings.GetMaster() * (IsMusic ? SoundsSettings.GetMusic() : SoundsSettings.GetAudio());
        _source.volume = volume;
    }

    private void PlayAudio(Audio Target)
    {   
        if (Target.Sounds.Length == 0)
        {
            Debug.LogWarning("Size of " + Target.KeyName + " - Lesser than 1");
            return;
        }

        _source.clip = Target.Sounds[UnityEngine.Random.Range(0, Target.Sounds.Length)];
        _source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        _source.Play();

        if (WaitingSoundEnd)
            StartCoroutine(WaitSoundEnd(Target));
    }

    private IEnumerator WaitSoundEnd(Audio Target)
    {
        NowPlaying = true;
        yield return new WaitForSeconds(_source.clip.length);
        if (IsMusic) PlayAudio(Target);
        NowPlaying = false;
    }

    public void Play(int WhatID)
    {
        if (WhatID < 0 || WhatID > Everything.Length)
        {
            Debug.LogWarning("ID - " + WhatID + " - doesn`t exist");
            return;
        }
        PlayAudio(Everything[WhatID]);
    }

    public void Play(string What)
    {
        if (NowPlaying) // If option WaitingSoundEnd is true we will wait end of sound
        {
            Debug.LogWarning("Sound is playing");
            return;
        }

        foreach (Audio current in Everything) // Search sound for keyword
        {
            if (current.KeyName == What) // if we found - we start play this sound array
            {
                PlayAudio(current);
                //Logger.Instance.LogWarning(this, What + " playing now");
                return;
            }
        }
        Debug.LogWarning("KeyName - " + What + " - doesn`t exist");
    }
}