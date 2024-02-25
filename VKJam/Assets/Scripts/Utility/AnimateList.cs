using UnityEngine;
using System.Collections;

public class AnimateList : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private void Awake()
    {
        if (_animator != null)
            _animator.StopPlayback();
    }
    public void Play(string WhichAnimation)
    {
        _animator.StartPlayback();
        _animator.Play(WhichAnimation);
        WaitAnimationEnd();
        _animator.StopPlayback();
    }
    private IEnumerator WaitAnimationEnd()
    {
        //Debug.Log(_animator.GetCurrentAnimatorClipInfo(0).Length);
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorClipInfo(0).Length);
    }
}