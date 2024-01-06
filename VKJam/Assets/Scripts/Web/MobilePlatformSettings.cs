using System.Runtime.InteropServices;
using UnityEngine;

public class MobilePlatformSettings : MonoBehaviour
{
    [DllImport("__Internal")] private static extern bool UnityPluginIsMobilePlatform();

    private void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (UnityPluginIsMobilePlatform() || Application.isMobilePlatform)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            QualitySettings.particleRaycastBudget = 265;
        }
#endif
    }
}
