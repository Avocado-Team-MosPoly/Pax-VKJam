using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class FirstStageManager : MonoBehaviour, IStageManager
{
    public event Action Started;
    public event Action Finished;

    [Header("First Part (Monster Texture and Description)")]
    [SerializeField] private GameObject firstContainer;
    [SerializeField] private float firstPartTime = 30f;

    [Header("Second Part (Ingredients)")]
    [SerializeField] private GameObject secondContainer;
    [SerializeField] private float secondPartTime = 30f;

    [Header("References")]
    [SerializeField] private ReadinessSystem readinessSystem;
    [SerializeField] private MonsterCardDrawing monsterCardDrawing;
    [SerializeField] private MonsterInfoInput monsterInfoInput;
    [SerializeField] private TextMeshProUGUI timerLabel;
    [Space(10)]
    [SerializeField] private NetworkCountdownTimer countdownTimer;

    public void StartStage()
    {
        firstContainer.SetActive(true);

        monsterCardDrawing.Enable();

        countdownTimer.ValueChanged += OnTimerTick;
        countdownTimer.Finished += OnFirstTimerFinished;
        if (NetworkManager.Singleton.IsServer)
            countdownTimer.Play(firstPartTime);

        Started?.Invoke();
    }

    public void FinishStage()
    {
        secondContainer.SetActive(false);

        monsterCardDrawing.Disable();
        countdownTimer.ValueChanged -= OnTimerTick;

        SendCardInfo();

        if (NetworkManager.Singleton.IsServer)
            readinessSystem.SetAllUnready();

        Finished?.Invoke();
    }

    private void OnTimerTick(float value)
    {
        timerLabel.text = value.ToString();
    }

    private void OnFirstTimerFinished()
    {
        monsterCardDrawing.Disable();

        firstContainer.SetActive(false);
        secondContainer.SetActive(true);

        countdownTimer.Finished -= OnFirstTimerFinished;
        countdownTimer.Finished += OnSecondTimerFinished;
        if (NetworkManager.Singleton.IsServer)
            countdownTimer.Play(secondPartTime);
    }

    private void OnSecondTimerFinished()
    {
        countdownTimer.Finished -= OnSecondTimerFinished;

        SecondModeManager.Instance.NextStage();
    }

    private void SendCardInfo()
    {
        string monsterName = monsterInfoInput.MonsterName;
        string monsterDescription = monsterInfoInput.Description;
        string[] monsterIngredients = monsterInfoInput.Ingredients;
        Texture2D monsterTexture = monsterCardDrawing.GetTexture();

        PackCrafter.Instance.SendCardInfo(monsterName, monsterDescription, monsterIngredients, monsterTexture);
    }
}