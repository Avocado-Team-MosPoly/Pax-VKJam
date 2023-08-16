using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Authentication : MonoBehaviour
{
    public static string PlayerName { get; private set; }
    public static string UserId { get; private set; }
    
    public static async void Authenticate()
    {
        string number = Random.Range(100, 1000).ToString();

        if (PlayerName == string.Empty || PlayerName == null)
        {
            PlayerName = "Player" + number;
        }
        if (UserId == string.Empty || UserId == null)
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
}