using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsAndSoundListConnector : MonoBehaviour
{
    [SerializeField] private ButtonWithSound[] buttonsWithSounds;

    private void Start()
    {
        if (BackgroundMusic.Instance == null)
            return;

        SoundList soundList = BackgroundMusic.Instance.GetComponentInChildren<SoundList>();
        if (soundList != null)
            AddSoundsToButtons(soundList);
    }

    private void AddSoundsToButtons(SoundList soundList)
    {
        foreach (ButtonWithSound buttonWithSound in buttonsWithSounds)
            buttonWithSound.button.onClick.AddListener(() => soundList.Play(buttonWithSound.musicName));
    }

    [Serializable]
    private struct ButtonWithSound
    {
        public Button button;
        public string musicName;
    }
}