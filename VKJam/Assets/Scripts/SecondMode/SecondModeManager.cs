using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class SecondModeManager : BaseSingleton<SecondModeManager>
{
    [SerializeField] private BaseStageManager[] stageManagers;

    private int currentStageIndex = -1;

    private void Start()
    {
        NetworkManager.Singleton.OnClientStarted += delegate
        {
            stageManagers[0].StartStage();
            currentStageIndex = 0;
        };
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
}