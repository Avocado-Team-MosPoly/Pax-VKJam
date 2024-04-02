using System.Runtime.InteropServices;
using UnityEngine;

public class MobilePlatformSettings : MonoBehaviour
{

    private void Awake()
    {
#if !UNITY_EDITOR
        if (Application.isMobilePlatform)
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.realtimeGICPUUsage = 75;
            QualitySettings.particleRaycastBudget = 512;
        }
#endif
    }
}