using System.CodeDom.Compiler;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BootManager : MonoBehaviour
{
    [SerializeField] private int defaultNickname = 333; // defauld = 333

    [Header("VK")]
    [SerializeField] private VK_Connect vkConnect;

    [Header("Server")]
    [SerializeField] private Php_Connect phpConnect;

    [Header("Multiplayer")]
    [SerializeField] private RelayManager relayManager;
    [SerializeField] private LobbyManager lobbyManager;

    [Header("Cards")]
    [SerializeField] private PackManager packManager;

    [Header("Custom")]
    [SerializeField] private CustomController customController;


    [Header("Loading UI")]
    [SerializeField] private TextMeshProUGUI statusLabel;
    [SerializeField] private Slider loadingSlider;

    [Header("Loading Statuses")]
    [SerializeField] private string vkConnect_Initialization;
    [SerializeField] private string phpConnect_Initialization;
    [SerializeField] private string phpConnect_Authentication;
    [SerializeField] private string getBDCardPacksData;
    [SerializeField] private string packManager_Initialization;
    [SerializeField] private string customController_Initialization;
    [SerializeField] private string authentication_Authentication;
    [SerializeField] private string relayManager_Initialization;
    [SerializeField] private string lobbyManager_Initialization;
    [SerializeField] private string sceneLoading;

    private IEnumerator Start()
    {
        Logger.Instance.Log(this, true.ToString());
        Logger.Instance.Log(this, false.ToString());

        loadingSlider.value = 0f;
        statusLabel.text = vkConnect_Initialization;
        yield return StartCoroutine(vkConnect.Init());

        UpdateLoadingStatus(phpConnect_Initialization);
        yield return StartCoroutine(phpConnect.Init());

        UpdateLoadingStatus(phpConnect_Authentication);
        if (UserData.UserId < 0)
        {
            Logger.Instance.LogWarning(this, "Unable to authenticate through VK id");
            yield return StartCoroutine(Php_Connect.Request_Auth(defaultNickname));
        }
        else
        {
            yield return StartCoroutine(Php_Connect.Request_Auth(UserData.UserId));
        }

        //send request whith card packs we have

        //for each pack we own send request which card in pack ownering

        //save prev logic

        string ownedCardsInPacks = null;
        UpdateLoadingStatus(getBDCardPacksData);
        yield return StartCoroutine(Php_Connect.Request_WhichCardInPackOwnering(packManager.Active.PackDBIndex, (string response) =>
        {
            ownedCardsInPacks = response;
        }));

        UpdateLoadingStatus(packManager_Initialization);
        yield return StartCoroutine(packManager.Init(ownedCardsInPacks));

        UpdateLoadingStatus(customController_Initialization);
        yield return StartCoroutine(customController.Init());

        UpdateLoadingStatus(authentication_Authentication);
        yield return Authentication.Authenticate(UserData.UserId.ToString(), UserData.UserName);

        UpdateLoadingStatus(relayManager_Initialization);
        yield return StartCoroutine(relayManager.Init());

        UpdateLoadingStatus(lobbyManager_Initialization);
        yield return StartCoroutine(lobbyManager.Init());

        UpdateLoadingStatus(sceneLoading);
        LoadStartScene();
    }

    private void UpdateLoadingStatus(string status)
    {
        loadingSlider.value += 1f / 10;
        statusLabel.text = status;
        //Logger.Instance.Log(this, $"Changed status: value = {loadingSlider.value}, text = {status}");
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