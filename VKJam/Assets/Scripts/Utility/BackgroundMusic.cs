using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    [SerializeField] private AudioClip defaultAudioClip;
    [SerializeField, Range(0f, 1f)] private float defaultAudioVolume = 1f;

    [Space(10)]
    [Tooltip("If scene is not in array it uses default audio clip. Set up element, if you want specific audio clip on it")]
    [SerializeField] private SceneAudioClip[] specificScenesAudioClips;

    private AudioSource source;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitAudio();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
            Destroy(gameObject);
    }

    private void InitAudio()
    {
        source = GetComponent<AudioSource>();
        source.clip = null;
        source.loop = true;

        Scene activeScene = SceneManager.GetActiveScene();
        OnSceneLoaded(activeScene, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        foreach (SceneAudioClip sceneAudio in specificScenesAudioClips)
        {
            if (sceneAudio.SceneName == scene.name)
            {
                if (source.clip == sceneAudio.AudioClip)
                    return;

                source.clip = sceneAudio.AudioClip;
                source.volume = sceneAudio.Volume;
                source.Play();

                return;
            }
        }

        if (source.clip == defaultAudioClip)
            return;

        source.clip = defaultAudioClip;
        source.volume = defaultAudioVolume;
        source.Play();
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

    [Serializable]
    private struct SceneAudioClip
    {
        public string SceneName;
        public AudioClip AudioClip;
        [Range(0f, 1f)] public float Volume;
    }
}