using System.Runtime.InteropServices;
using UnityEngine;

public class MobileKeyboard : MonoBehaviour
{
    public void Open()
    {
#if !UNITY_EDITOR
        if (Application.isMobilePlatform)
            TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Default);
#endif
        return;
    }
}