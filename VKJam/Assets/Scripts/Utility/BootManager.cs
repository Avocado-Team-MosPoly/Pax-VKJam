using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BootManager : MonoBehaviour
{
    [Header("Server Connection Data")]
    [SerializeField] private bool useDefaultNickname;
    [SerializeField] private int defaultNickname = 333; // default = 333
    [SerializeField] private bool canPlaySingle;
    
    [SerializeField, Tooltip("Time between connection attempts in seconds"), Min(0f)] private float connectionTimeout = 1f;
    [SerializeField] private int maxConnectionAttempts = 10;

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

    [Header("Disclamer")]
    [SerializeField] private bool showDisclaimer = true;
    [SerializeField] private GameObject disclaimerPanel;
    [SerializeField] private float disclaimerShowTime = 3.5f;

    [Header("Exception")]
    [SerializeField] private GameObject exceptionPanel;
    [SerializeField] private TextMeshProUGUI exceptionLabel;

    private Coroutine loadingCoroutine;

    private void Start()
    {
        StartLoading();
    }

    private IEnumerator Loading()
    {
        // Disclaimer

        StartCoroutine(ShowDisclaimer(showDisclaimer));

        // VK

        loadingSlider.value = 0f;
        statusLabel.text = vkConnect_Initialization;
        yield return StartCoroutine(vkConnect.Init());

        // Dedicated Server

        UpdateLoadingStatus(phpConnect_Initialization);
        yield return StartCoroutine(phpConnect.Init());

        bool loadTutorial = true;

        #region Authentication on server

        int connectionAttemptNumber = 0;
        Action<bool> successAuthentication = (bool isFirstTime) =>
        {
            Authentication.IsLoggedInThroughVK = !useDefaultNickname;
            loadTutorial = isFirstTime;
        };
        Action<string> unsuccesAuthentication = (string exception) =>
        {
            exceptionPanel.SetActive(true);

            if (exception == "LogIn through several devices exception")
            {
                exceptionLabel.text = "�� ��� ��������� ���� � ����� ��������";
            }
            else
            {
                exceptionLabel.text = "����������� ������";
            }

            StopLoading();
            DestroyManagers();
            Destroy(this);
        };

        UpdateLoadingStatus(phpConnect_Authentication);
        for (connectionAttemptNumber = 0; connectionAttemptNumber < maxConnectionAttempts; connectionAttemptNumber++)
        {
            if (!useDefaultNickname && UserData.UserId < 0)
            {
                yield return new WaitForSeconds(connectionTimeout);
                continue;
            }

            yield return StartCoroutine(Php_Connect.Request_Auth(
                useDefaultNickname ? defaultNickname : UserData.UserId,
                successAuthentication, unsuccesAuthentication));

            if (Php_Connect.PHPisOnline)
                break;

            yield return new WaitForSeconds(connectionTimeout);
        }
        if (connectionAttemptNumber >= maxConnectionAttempts - 1 && !Php_Connect.PHPisOnline)
        {
            Logger.Instance.LogError(this, $"Unable to connect to dedicated server using {(useDefaultNickname ? "Default nickname" : "VK uid")}");
            NotificationSystem.Instance.SendLocal($"�� ������� ������������ � �������� ��������� {(useDefaultNickname ? "����������� �����" : "�� uid")}");
        }

        #endregion

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

        // Load Ads Data

        yield return StartCoroutine(AdManager.Init());

        // Unity Services

        UpdateLoadingStatus(authentication_Authentication);
        string unityServicesId = (useDefaultNickname ? defaultNickname : UserData.UserId).ToString();
        string unityServicesNickname = useDefaultNickname ? defaultNickname.ToString() : UserData.UserName;
        yield return Authentication.Authenticate(unityServicesId, unityServicesNickname);

        UpdateLoadingStatus(relayManager_Initialization);
        yield return StartCoroutine(relayManager.Init());

        UpdateLoadingStatus(lobbyManager_Initialization);
        yield return StartCoroutine(lobbyManager.Init());

        UpdateLoadingStatus(sceneLoading);

        if (canPlaySingle)
            GameLaunchParams.MinPlayersToLaunchCount = 1;
        
        LoadStartScene(loadTutorial);
    }

    private void DestroyManagers()
    {
        Destroy(vkConnect);
        Destroy(phpConnect);
        Destroy(relayManager);
        Destroy(lobbyManager);
        Destroy(packManager);
        Destroy(customController);
    }

    private void StartLoading()
    {
        if (loadingCoroutine != null)
            return;

        loadingCoroutine = StartCoroutine(Loading());
    }

    private void StopLoading()
    {
        if (loadingCoroutine == null)
            return;

        StopCoroutine(loadingCoroutine);
        loadingCoroutine = null;
    }

    private void UpdateLoadingStatus(string status)
    {
        loadingSlider.value += 1f / 10;
        statusLabel.text = status;
        //Logger.Instance.Log(this, $"Changed status: value = {loadingSlider.value}, text = {status}");
    }

    private void LoadStartScene(bool loadTutorial)
    {
        if (loadTutorial)
            SceneLoader.Load("TutorialScene");
        else
            SceneLoader.Load("Menu");
    }

    private IEnumerator ShowDisclaimer(bool status)
    {
        if (status)
        {
            disclaimerPanel.SetActive(true);
            yield return new WaitForSeconds(disclaimerShowTime);
            disclaimerPanel.SetActive(false);
        }
        else
            disclaimerPanel.SetActive(false);
    }
}