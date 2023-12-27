using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WebFunctions : MonoBehaviour
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

    public void Paste(string text)
    {
        Logger.Instance.Log(this, $"Pasted text: {text}");

        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
            return;

        if (EventSystem.current.currentSelectedGameObject.TryGetComponent(out TMP_InputField inputField))
            inputField.text = text;
    }
}