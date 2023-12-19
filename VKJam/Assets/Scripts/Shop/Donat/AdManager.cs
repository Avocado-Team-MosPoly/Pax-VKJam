using System;
using UnityEngine;

public class AdManager
{
    private const string AdsWatchedKey = "AdsWatched";
    private const string AdLastWatchedTimeKey = "AdLastWatchedTime";
    private const int MaxAdsPerDay = 3;

    public static bool CanShowAd()
    {
        int adsWatchedToday = PlayerPrefs.GetInt(AdsWatchedKey, 0);
        DateTime lastWatched = GetLastAdWatchedTime();

        // ≈сли текущий день не совпадает с датой последнего просмотра, сбросить счетчик
        if (lastWatched.Date != DateTime.UtcNow.Date)
        {
            PlayerPrefs.SetInt(AdsWatchedKey, 0);
            adsWatchedToday = 0;
        }

        // ѕровер€ем, не превысили ли мы максимальное количество просмотров
        return adsWatchedToday < MaxAdsPerDay;
    }
    public static int GetAdsWatchedToday()
    {
        return PlayerPrefs.GetInt(AdsWatchedKey, 0);
    }
    public static void OnAdWatched()
    {
        int adsWatchedToday = PlayerPrefs.GetInt(AdsWatchedKey, 0);
        PlayerPrefs.SetInt(AdsWatchedKey, adsWatchedToday + 1);
        SetLastAdWatchedTime(DateTime.UtcNow);
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