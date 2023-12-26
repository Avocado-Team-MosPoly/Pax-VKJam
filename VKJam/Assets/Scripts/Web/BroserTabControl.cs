using UnityEngine;

public class BroserTabControl : MonoBehaviour
{
    private void Start()
    {
        OnTabFocus();
    }

    public void OnTabFocus()
    {
        SoundList.MuteObserver.Value = false;
        BackgroundMusic.Instance.Mute = false;
    }

    public void OnTabBlur()
    {
        SoundList.MuteObserver.Value = true;
        BackgroundMusic.Instance.Mute = true;
    }
}
