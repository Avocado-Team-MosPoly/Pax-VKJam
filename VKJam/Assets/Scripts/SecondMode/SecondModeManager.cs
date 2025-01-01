using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SecondModeManager : BaseSingleton<SecondModeManager>
{
    [Header("Network Behaviours")]
    [SerializeField] private NetworkBehaviour[] networkBehaviours;

    [Header("Systems")]
    [SerializeField] private ReadinessSystem readinessSystem;
    [SerializeField] private BaseStageManager[] stageManagers;

    // [SerializeField] private <note manager>

    [Header("Main Game Layout")]
    [SerializeField] private GameObject mainGameRoot;

    private int currentStageIndex = -1;

    private SecondModeGuessSystem guessSystem;

    public SecondModeStage Stage { get; set; }
    public ReadinessSystem ReadinessSystem => readinessSystem;
    public SecondModeGuessSystem GuessSystem => guessSystem = guessSystem != null ? guessSystem : GameManager.Instance.GuessSystem as SecondModeGuessSystem;
    public GameObject MainGameRoot => mainGameRoot;

    private void Start()
    {
        // TODO: remove if statement and leave just init when mode develop ends
        if (NetworkManager.Singleton.IsClient)
            StartCoroutine(Init());
        else
            NetworkManager.Singleton.OnClientStarted += () => StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        int index = 0;
        while (index < networkBehaviours.Length)
        {
            if (networkBehaviours[index].IsSpawned)
                index++;
            else
                yield return null;
        }

        Stage = SecondModeStage.Waiting;

        stageManagers[0].StartStage();
        currentStageIndex = 0;
    }

    public void SetStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= stageManagers.Length - 1)
            return;

        stageManagers[currentStageIndex].FinishStage();
        stageManagers[stageIndex].StartStage();

        currentStageIndex = stageIndex;
    }

    public void NextStage()
    {
        SetStage(currentStageIndex + 1);
    }

    public void PreviousStage()
    {
        SetStage(currentStageIndex - 1);
    }

    public void ReturnToLobby()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError($"[{nameof(SecondModeManager)}] Return to Lobby available only on server");
            return;
        }

        RelayManager.Instance.ReturnToLobby();
    }

    public void SendAnswer(string answer)
    {
        GuessSystem.SendAnswerServerRpc(answer, new ServerRpcParams());
    }
}