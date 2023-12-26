using UnityEngine;

public class BroserTabControl : MonoBehaviour
{
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
