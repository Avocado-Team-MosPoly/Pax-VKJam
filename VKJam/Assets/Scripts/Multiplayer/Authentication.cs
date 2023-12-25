using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Authentication : MonoBehaviour
{
    public static string PlayerName { get; private set; }
    public static string UserId { get; private set; }

    public static bool IsLoggedInThroughVK;// { get; private set; }

    public static async Task Authenticate(string userId, string playerName)
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            Logger.Instance.Log(typeof(Authentication), "Unity Services alredy initialized");
        }
        else
        {
            UserId = userId;
            PlayerName = playerName.Replace('¸', 'å');
            string number = Random.Range(100, 1000).ToString();

            if (string.IsNullOrEmpty(PlayerName))
                PlayerName = "Player" + number;

            if (string.IsNullOrEmpty(UserId))
                UserId = number;

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
            Logger.Instance.Log(typeof(Authentication), $"Signed in. Your id is {AuthenticationService.Instance.PlayerId}, player name is {PlayerName}");
        };

        Logger.Instance.Log(typeof(Authentication), "Sign in in progress...");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public static async Task ChangePlayerName(string playerName)
    {
        PlayerName = playerName;

        if (AuthenticationService.Instance.IsAuthorized)
            await AuthenticationService.Instance.UpdatePlayerNameAsync(PlayerName);
    }

    //public static void ChangeProfile(string userId)
    //{
    //    UserId = userId;

    //    if (!AuthenticationService.Instance.IsAuthorized)
    //        AuthenticationService.Instance.SwitchProfile(userId);
    //}

    //public static void SetVKProfile(string userId, string playerName)
    //{
    //    if (AuthenticationService.Instance.IsAuthorized)
    //    {
    //        Logger.Instance.LogWarning(typeof(Authentication), $"{nameof(AuthenticationService)} already authorized");
    //        return;
    //    }

    //    UserId = userId;
    //    PlayerName = playerName;

    //    AuthenticationService.Instance.SwitchProfile(UserId);

    //    IsLoggedInThroughVK = true;
    //}

    public static void SignOut()
    {
        AuthenticationService.Instance.SignOut(true);
        //IsLoggedInThroughVK = false;
    }
}