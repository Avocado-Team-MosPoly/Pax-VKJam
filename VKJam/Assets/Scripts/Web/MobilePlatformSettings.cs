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
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.realtimeGICPUUsage = 75;
            QualitySettings.particleRaycastBudget = 512;
        }
#endif
    }
}