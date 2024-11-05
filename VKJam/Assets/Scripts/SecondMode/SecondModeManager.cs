using Unity.Netcode;
using UnityEngine;

public class SecondModeManager : BaseSingleton<SecondModeManager>
{
    [SerializeField] private IStageManager[] stageManagers;

    private int currentStageIndex = -1;

    private void Start()
    {
        stageManagers[0].StartStage();
        currentStageIndex = 0;
    }

    public void NextStage()
    {
        if (currentStageIndex < 0 || currentStageIndex >= stageManagers.Length - 1)
            return;

        stageManagers[currentStageIndex].FinishStage();
        stageManagers[++currentStageIndex].StartStage();
    }

    public void PreviousStage()
    {
        if (currentStageIndex < 1 || currentStageIndex >= stageManagers.Length)
            return;

        stageManagers[currentStageIndex].FinishStage();
        stageManagers[--currentStageIndex].StartStage();
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
}