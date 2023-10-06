using System.Collections;
using UnityEngine;

namespace Tiractor.Sound
{
    [RequireComponent(typeof(AudioSource))]

    public class SoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;
        private bool NowPlaying;
        [Tooltip("While true new sound will not playing if old still playing")] [SerializeField] private bool WaitingSoundEnd = true;
        [SerializeField] private SoundListNW Sound;
        private void Start()
        {
            NowPlaying = false;
            if (_source == null) _source = GetComponent<AudioSource>(); // Saving standart output Source for more quickly 
            if (Sound == null)
            {
                Debug.LogWarning(gameObject.name + " - this object wait his soundlist");
                return;
            }
            ChangeVolume(); // For get pre-setting
            SoundsSettings._VolumeChanging += ChangeVolume; // Now SoundSettings can change Volume, when changes Value in him
        }
        public void SetSoundList(SoundListNW New)
        {
            if (Sound == null)
            {
                SoundsSettings._VolumeChanging += ChangeVolume; // Now SoundSettings can change Volume, when changes Value in him
            }
            if (_source == null) _source=GetComponent<AudioSource>();
            Sound = New;
            ChangeVolume();
        }
        public void ChangeVolume()
        {
            // Cause I use static var in Settings I can interact with base class
            _source.volume = SoundsSettings.GetMaster() * (Sound.IsMusic ? SoundsSettings.GetMusic() : SoundsSettings.GetAudio());
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
            if (Sound.IsMusic) PlayAudio(Target);
            NowPlaying = false;
        }
        public void Play(int WhatID)
        {
            if (WhatID > Sound.Everything.Length)
            {
                Debug.LogWarning("ID - " + WhatID + " - doesn`t exist");
                return;
            }
            PlayAudio(Sound.Everything[WhatID]);
        }
        public void Play(string What)
        {
            if (NowPlaying) // If option WaitingSoundEnd is true we will wait end of sound
            {
                Debug.LogWarning("Sound is playing");
                return;
            }
            foreach (Audio current in Sound.Everything) // Search sound for keyword
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
}