using UnityEngine;

public class AnimateList : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    public void Play(string WhichAnimation)
    {
        _animator.Play(WhichAnimation);
    }
}