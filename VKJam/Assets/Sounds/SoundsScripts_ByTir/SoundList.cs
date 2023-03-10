using UnityEngine;
using System.Collections;
[RequireComponent(typeof(AudioSource))]
public class SoundList : MonoBehaviour
{
    [System.Serializable]
    public struct Audio
    {
        [Tooltip("This used for detecting this elements for Play function")] public string KeyName;
        public AudioClip[] Sounds;
    }

    [Header("Audio settings")]
    [Tooltip("This created for correct using Settings")] public bool IsMusic;
    [Tooltip("While true new sound will not playing if old still playing")] [SerializeField] private bool WaitingSoundEnd = true;

    [Header("Audio list")]
    [SerializeField] private Audio[] Everything;
    private AudioSource _source;



    private bool NowPlaying;
    private void Awake()
    {
        NowPlaying = false;
        _source = GetComponent<AudioSource>(); // Saving standart output Source for more quickly 
        ChangeVolume(); // For get pre-setting
        SoundsSettings._VolumeChanging += ChangeVolume; // Now SoundSettings can change Volume, when changes Value in him
    }
    public void ChangeVolume()
    {
        // Cause I use static var in Settings I can interact with base class
        _source.volume = SoundsSettings.GetMaster() * (IsMusic ? SoundsSettings.GetMusic() : SoundsSettings.GetAudio());
    }
    private void PlayAudio(Audio Target)
    {
        if (Target.Sounds.Length == 0)
        {
            Debug.LogWarning("Size of " + Target.KeyName + " - Lesser than 1");
            return;
        }
        _source.clip = Target.Sounds[Random.Range(0, Target.Sounds.Length)];
        _source.Play();
        if (WaitingSoundEnd) WaitSoundEnd(Target);
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
        if (WhatID > Everything.Length)
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
                return;
            }
        }
        Debug.LogWarning("KeyName - " + What + " - doesn`t exist");
    }
}
