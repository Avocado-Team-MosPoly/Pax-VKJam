using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.VisualScripting;
using UnityEngine;
using WebSocketSharp;

public class Authentication : MonoBehaviour
{
    public static string PlayerName { get; private set; }
    public static string UserId { get; private set; }

    public static bool IsLoggedInThroughVK { get; private set; }

    public static async void Authenticate()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("Unity Services alredy initialized");
            return;
        }

        string number = Random.Range(100, 1000).ToString();

        if (PlayerName == string.Empty || PlayerName == null)
        {
            PlayerName = "Player" + number;
        }
        if (UserId.IsNullOrEmpty())
        {
            UserId = number;
        }

        Logger.Instance.Log("Player Name: " + PlayerName);
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(UserId);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed In. Your Id is " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
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