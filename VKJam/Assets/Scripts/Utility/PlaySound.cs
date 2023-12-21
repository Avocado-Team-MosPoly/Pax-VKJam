using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField] private SoundList soundList;

    public void Play(string sfxName)
    {
        soundList.Play(sfxName);
    }
}