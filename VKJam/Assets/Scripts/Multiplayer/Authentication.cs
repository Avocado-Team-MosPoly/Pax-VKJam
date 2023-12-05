using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.VisualScripting;
using UnityEngine;

public class Authentication : MonoBehaviour
{
    public static string PlayerName { get; private set; }
    public static string UserId { get; private set; }

    public static bool IsLoggedInThroughVK { get; private set; }

    public static async void Authenticate()
    {
        Logger.Instance.Log(typeof(Authentication), "Authentication started");
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            Logger.Instance.Log(typeof(Authentication), "Unity Services alredy initialized");
        }
        else
        {
            string number = Random.Range(100, 1000).ToString();

            if (PlayerName == string.Empty || PlayerName == null)
            {
                PlayerName = "Player" + number;
            }
            if (string.IsNullOrEmpty(UserId))
            {
                UserId = number;
            }

            InitializationOptions initializationOptions = new();
            initializationOptions.SetProfile(UserId);

            await UnityServices.InitializeAsync(initializationOptions);
        }

        if (AuthenticationService.Instance.IsAuthorized)
        {
            Logger.Instance.Log(typeof(Authentication), "You alredy authorized");
            return;
        }

        AuthenticationService.Instance.SignedIn += () =>
        {
        };
        Logger.Instance.Log(typeof(Authentication), "Sign in in progress...");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Logger.Instance.Log(typeof(Authentication), $"Signed in. Your id is {AuthenticationService.Instance.PlayerId}, player name is {PlayerName}");
    }

    public static void ChangePlayerName(string playerName)
    {
        PlayerName = playerName;
    }

    public static void ChangeProfile(string userId)
    {
        UserId = userId;

        if (AuthenticationService.Instance.IsAuthorized)
            AuthenticationService.Instance.SwitchProfile(userId);
    }

    public static void LogInVK(string userId, string playerName)
    {
        UserId = userId;
        PlayerName = playerName;

        if (AuthenticationService.Instance.IsAuthorized)
            AuthenticationService.Instance.SwitchProfile(playerName);

        IsLoggedInThroughVK = true;
    }

    public static void SignOut()
    {
        AuthenticationService.Instance.SignOut(true);
    }
}