using UnityEngine;
using System.Collections;
namespace Tiractor.Sound
{
    [System.Serializable]
    public struct Audio
    {
        [Tooltip("This used for detecting this elements for Play function")] public string KeyName;
        public AudioClip[] Sounds;
    }
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Sound List", menuName = "Sound list", order = 61)]
    public class SoundListNW : ScriptableObject
{
    [Header("Audio settings")]
    [Tooltip("This created for correct using Settings")] public bool IsMusic;
    [Tooltip("Which Tags must have this SoundList")] [SerializeField] private string[] Tags;

    [Header("Audio list")]
    [SerializeField] public Audio[] Everything;
    
    private void OnValidate()
    {
            foreach (string current in Tags)
            {
                GameObject[] gos;
                gos = GameObject.FindGameObjectsWithTag(current);
                foreach (GameObject target in gos)
                {
                    SoundController temp = target.GetComponent<SoundController>();
                    
                    if (temp == null)
                    {
                        temp = target.AddComponent<SoundController>();
                    }
                    
                    temp.SetSoundList(this);
                }
            }
    }
}
}
