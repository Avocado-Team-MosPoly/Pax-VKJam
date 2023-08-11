using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Authentication : MonoBehaviour
{
    public static string PlayerName { get; private set; }

    public static async void Authenticate()
    {
        if (PlayerName == string.Empty)
        {
            PlayerName = Random.Range(100, 1000).ToString();
        }
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(PlayerName);
        
        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed In. Your Id is " + AuthenticationService.Instance.PlayerId);

            //LobbyManager.Instance.ListLobbies();
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public static void ChangePlayerName(string playerName)
    {
        PlayerName = playerName;
        
        if (AuthenticationService.Instance.IsAuthorized)
            AuthenticationService.Instance.SwitchProfile(PlayerName);
    }
}