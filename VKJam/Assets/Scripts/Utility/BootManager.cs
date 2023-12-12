using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BootManager : MonoBehaviour
{
    [Header("VK")]
    [SerializeField] private VK_Connect vkConnect;

    [Header("Server")]
    [SerializeField] private Php_Connect phpConnect;

    [Header("Multiplayer")]
    [SerializeField] private RelayManager relayManager;
    [SerializeField] private LobbyManager lobbyManager;

    [Header("Custom")]
    [SerializeField] private CustomController customController;


    [Header("Loading UI")]
    [SerializeField] private Image loadingBar;

    private IEnumerator Start()
    {
        loadingBar.fillAmount = 0f;

        yield return StartCoroutine(vkConnect.Init());
        loadingBar.fillAmount += 1f / 7;
        yield return StartCoroutine(phpConnect.Init());
        loadingBar.fillAmount += 1f / 7;
        yield return StartCoroutine(relayManager.Init());
        loadingBar.fillAmount += 1f / 7;
        yield return StartCoroutine(lobbyManager.Init());
        loadingBar.fillAmount += 1f / 7;
        yield return StartCoroutine(customController.Init());
        loadingBar.fillAmount += 1f / 7;

        if (UserData.UserId < 0)
        {
            yield return StartCoroutine(Php_Connect.Request_Auth(333));
        }
        else
        {
            yield return StartCoroutine(Php_Connect.Request_Auth(UserData.UserId));
        }
        loadingBar.fillAmount += 1f / 7;

        yield return Authentication.Authenticate(UserData.UserId.ToString(), UserData.UserName);
        loadingBar.fillAmount = 1f;

        LoadStartScene();
    }

    private void LoadStartScene()
    {
        int tutorialCompleted = PlayerPrefs.GetInt(nameof(tutorialCompleted), 0);

        if (tutorialCompleted == 1)
        {
            SceneLoader.Load("Menu");
        }
        else
        {
            PlayerPrefs.SetInt(nameof(tutorialCompleted), 1);
            SceneLoader.Load("TutorialScene");
        }
    }
}