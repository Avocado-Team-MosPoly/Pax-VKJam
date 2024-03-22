using System;
using System.Collections;
using UnityEngine;

public class AdManager
{
    public static int WatchedAdsThisDay { get; private set; } = 0;
    public static int MaxAdsToWatchPerDay { get; private set; } = 5;

    private const string AdsWatchedKey = "AdsWatched";
    private const string AdLastWatchedTimeKey = "AdLastWatchedTime";
    private const int MaxAdsPerDay = 5;

    public static IEnumerator Init()
    {
        yield return Php_Connect.Instance.StartCoroutine(Php_Connect.Request_AdsCount((response) =>
        {
            string[] nums = response.Split("/");
            switch (nums.Length)
            {
                case 1:
                {
                    if (int.TryParse(nums[0], out int value11))
                        WatchedAdsThisDay = value11;

                    break;
                }
                case 2:
                {
                    if (int.TryParse(nums[0], out int value21) && int.TryParse(nums[1], out int value22))
                    {
                        WatchedAdsThisDay = value21;
                        MaxAdsToWatchPerDay = value22;
                    }

                    break;
                }
            }
        }, null));
    }

    public static bool CanShowAd()
    {
        return WatchedAdsThisDay < MaxAdsToWatchPerDay;
        //int adsWatchedToday = PlayerPrefs.GetInt(AdsWatchedKey, 0);
        //DateTime lastWatched = GetLastAdWatchedTime();

        //// ���� ������� ���� �� ��������� � ����� ���������� ���������, �������� �������
        //if (lastWatched.Date != DateTime.UtcNow.Date)
        //{
        //    PlayerPrefs.SetInt(AdsWatchedKey, 0);
        //    adsWatchedToday = 0;
        //}

        //// ���������, �� ��������� �� �� ������������ ���������� ����������
        //return adsWatchedToday < MaxAdsPerDay;
    }
    public static int GetAdsWatchedToday()
    {
        return PlayerPrefs.GetInt(AdsWatchedKey, 0);
    }
    public static void OnAdWatched()
    {
        WatchedAdsThisDay++;
        if (WatchedAdsThisDay > MaxAdsPerDay)
            WatchedAdsThisDay = MaxAdsPerDay;

        //int adsWatchedToday = PlayerPrefs.GetInt(AdsWatchedKey, 0);
        //PlayerPrefs.SetInt(AdsWatchedKey, adsWatchedToday + 1);
        //SetLastAdWatchedTime(DateTime.UtcNow);
    }

    private static DateTime GetLastAdWatchedTime()
    {
        string lastWatchedString = PlayerPrefs.GetString(AdLastWatchedTimeKey, null);
        if (string.IsNullOrEmpty(lastWatchedString))
        {
            return DateTime.MinValue;
        }

        return DateTime.Parse(lastWatchedString, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    private static void SetLastAdWatchedTime(DateTime time)
    {
        string timeString = time.ToString("o"); // "o" format specifies an ISO 8601 format
        Logger.Instance.LogError(typeof(AdManager), "timeString = " + timeString);
        PlayerPrefs.SetString(AdLastWatchedTimeKey, timeString);
        PlayerPrefs.Save();
    }
}