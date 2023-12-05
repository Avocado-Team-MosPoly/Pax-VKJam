using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    public float Volume
    {
        get
        {
            return source.volume;
        }
        set
        {
            source.volume = Mathf.Clamp01(value);
        }
    }

    public bool Mute
    {
        get
        {
            return source.mute;
        }
        set
        {
            source.mute = value;
        }
    }

    [SerializeField] private AudioClip defaultAudioClip;
    [SerializeField, Range(0f, 1f)] private float defaultAudioVolume = 1f;

    [Space(10)]
    [Tooltip("If scene is not in array it uses default audio clip. Set up element, if you want specific audio clip on it")]
    [SerializeField] private SceneAudioClips[] specificScenesAudioClips;

    private AudioSource source;
    private string currentMusicId;

    private string KEY_VOLUME = "Volume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitAudio();
            LoadData();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SaveData();
    }

    private void InitAudio()
    {
        source = GetComponent<AudioSource>();
        source.clip = null;
        source.loop = true;

        Scene activeScene = SceneManager.GetActiveScene();
        OnSceneLoaded(activeScene, LoadSceneMode.Single);
    }

    private void SaveData()
    {
        PlayerPrefs.SetFloat(KEY_VOLUME, Volume);
    }

    private void LoadData()
    {
        Volume = PlayerPrefs.GetFloat(KEY_VOLUME, defaultAudioVolume);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (Play(scene.name + ":default"))
            return;

        if (source.clip == defaultAudioClip)
            return;

        SetMusic("default-default", defaultAudioClip, defaultAudioVolume);
    }

    private void SetMusic(string musicId, AudioClip audioClip, float volume)
    {
        source.clip = audioClip;
        source.volume = volume;
        currentMusicId = musicId;

        source.Play();
    }

    /// <summary> You should be on scene that contains 'musicName' to play it </summary>
    /// <returns> True if music found and setted, false is not </returns>
    public bool Play(string musicName)
    {
        Scene activeScene = SceneManager.GetActiveScene();

        foreach (SceneAudioClips sac in specificScenesAudioClips)
        {
            if (sac.sceneName == activeScene.name)
            {
                if (musicName == "default")
                {
                    SetMusic(activeScene.name + ":" + musicName, sac.defaultAudioClip, sac.defaultAudioClipVolume);
                    return true;
                }

                foreach (Clip clip in sac.namedAudioClips)
                {
                    if (clip.name == musicName)
                    {
                        if (currentMusicId.Split('-')[1] == musicName && source.clip == clip.clip)
                        {
                            Logger.Instance.LogWarning(this, $"{musicName} clip already playing");
                            return false;
                        }

                        SetMusic(activeScene.name + ":" + musicName, clip.clip, clip.volume);
                        return true;
                    }
                }

                break;
            }
        }

        return false;
    }

    public void Play()
    {
        if (source.clip == null)
            InitAudio();
        else
            source.Play();
    }

    public void Pause()
    {
        source.Pause();
    }

    public void Stop()
    {
        source.Stop();
    }

    public void ResetProgress()
    {
        source.time = 0f;
    }

    [Serializable]
    private struct SceneAudioClips
    {
        public string sceneName;

        public AudioClip defaultAudioClip;
        [Range(0f, 1f)] public float defaultAudioClipVolume;

        public Clip[] namedAudioClips;
    }

    [Serializable]
    private struct Clip
    {
        public string name;

        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }
}