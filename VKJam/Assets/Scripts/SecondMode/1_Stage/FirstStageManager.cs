using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FirstStageManager : BaseStageManager
{
    [Header("First Part (Monster Texture and Description)")]
    [SerializeField] private float firstPartDuration = 30f;
    [SerializeField] private GameObject firstContainer;
    [SerializeField] private GameObject painterControlsContainer;

    [Header("Second Part (Ingredients)")]
    [SerializeField] private float secondPartDuration = 30f;
    [SerializeField] private GameObject secondContainer;
    [Space(10)]
    [SerializeField] private RawImage monsterPainting;
    [SerializeField] private TextMeshProUGUI monsterName;
    [SerializeField] private TextMeshProUGUI monsterDescription;

    [Header("General")]
    [SerializeField] private NetworkCountdownTimer countdownTimer;
    [SerializeField] private ReadinessSystem readinessSystem;
    [SerializeField] private MonsterCardDrawing monsterCardDrawing;
    [SerializeField] private MonsterInfoInput monsterInfoInput;
    [Space(10)]
    [SerializeField] private TextMeshProUGUI timerLabel;

    public override void StartStage()
    {
        firstContainer.SetActive(true);

        monsterCardDrawing.Init();
        monsterCardDrawing.Enable();


        countdownTimer.ValueChanged += OnTimerTick;
        countdownTimer.Finished += OnFirstTimerFinished;
        if (NetworkManager.Singleton.IsServer)
        {
            countdownTimer.Play(firstPartDuration);

            readinessSystem.SetAllUnready();
            readinessSystem.AllReady += OnAllReady;
        }

        gameObject.SetActive(true);
        base.StartStage();
    }

    public override void FinishStage()
    {
        secondContainer.SetActive(false);

        monsterCardDrawing.Disable();

        countdownTimer.ValueChanged -= OnTimerTick;

        SendCardInfo();

        if (NetworkManager.Singleton.IsServer)
        {
            readinessSystem.AllReady -= OnAllReady;
        }

        gameObject.SetActive(false);
        base.FinishStage();
    }

    private void OnAllReady()
    {
        countdownTimer.Finish();
        readinessSystem.SetAllUnready();
    }

    private void OnTimerTick(float value)
    {
        timerLabel.text = ((int)value).ToString();
    }

    private void OnFirstTimerFinished()
    {
        monsterCardDrawing.Disable();

        monsterName.text = monsterInfoInput.Name;
        monsterDescription.text = monsterInfoInput.Description;
        monsterPainting.texture = monsterCardDrawing.GetTexture();

        firstContainer.SetActive(false);
        painterControlsContainer.SetActive(false);
        secondContainer.SetActive(true);

        countdownTimer.Finished -= OnFirstTimerFinished;
        countdownTimer.Finished += OnSecondTimerFinished;
        if (NetworkManager.Singleton.IsServer)
        {
            readinessSystem.SetAllUnready();
            countdownTimer.Play(secondPartDuration);
        }
    }

    private void OnSecondTimerFinished()
    {
        countdownTimer.Finished -= OnSecondTimerFinished;

        SecondModeManager.Instance.NextStage();
    }

    private void SendCardInfo()
    {
        string monsterName = monsterInfoInput.Name;
        string monsterDescription = monsterInfoInput.Description;
        string[] monsterIngredients = monsterInfoInput.Ingredients;
        Texture2D monsterTexture = monsterCardDrawing.GetTexture();

        PackCrafter.Instance.SendCardInfo(monsterName, monsterDescription, monsterIngredients, monsterTexture);
    }
}