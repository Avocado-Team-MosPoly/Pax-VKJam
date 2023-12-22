using System.Runtime.InteropServices;
using UnityEngine;

public class MobileKeyboard : MonoBehaviour
{
    [DllImport("__Internal")] private static extern bool UnityPluginIsMobilePlatform();

    public void Open()
    {
#if UNITY_EDITOR
        return;
#elif UNITY_WEBGL
        if (UnityPluginIsMobilePlatform())
            TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Default;
#endif
    }
}