using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.Events;

public class MainGameTimer : NetworkBehaviour
{
    [SerializeField] private TMP_Text ShowTime;
    public NetworkVariable<int> NetworkTime = new(0);
    private bool isTimePaused = false;
    [SerializeField] private Hint showRecepiesUI;

    private int ingredientGuessTime = 45;
    private int monsterGuessTime = 120;

    private int roundTime;

    private Coroutine serverClockCoroutine = null;

    [HideInInspector] public UnityEvent OnExpired;

    public void Init(GameConfigSO gameConfig)
    {
        ingredientGuessTime = gameConfig.TimeForIngredientGuess;
        monsterGuessTime = gameConfig.TimeForMonsterGuess;

        roundTime = ingredientGuessTime;
        ShowTime.text = ToTimeFormat(roundTime);

        NetworkTime.OnValueChanged += OnTimerChange;

        if (NetworkManager.Singleton.IsServer)
            NetworkTime.Value = roundTime;
    }

    private IEnumerator Clock()
    {
        //Debug.Log("Clock on client " + NetworkManager.Singleton.LocalClientId);

        while (IsServer)
        {
            //Debug.Log("Clock on server " + NetworkManager.Singleton.LocalClientId);

            if (isTimePaused)
            {
                yield return new WaitForSeconds(1);
                continue;
            }

            if (NetworkTime.Value <= 0)
            {
                StopTimer();

                OnExpired?.Invoke();

                ResetToDefault();
            }
            else
                NetworkTime.Value -= 1;
            
            yield return new WaitForSeconds(1);
        }
    }

    private void ResetToDefault()
    {
        NetworkTime.Value = roundTime;
    }

    private void OnTimerChange(int previousValue, int newValue)
    {
        if (newValue < 0)
            return;

        ShowTime.text = ToTimeFormat(newValue);
    }

    private string ToTimeFormat(int seconds)
    {
        string timeString = (seconds / 60).ToString() + ":";
        int onlySeconds = seconds % 60;
        
        timeString += onlySeconds < 10 ? "0" + onlySeconds : onlySeconds;

        return timeString;
    }

    public void StartTimer()
    {
        if (serverClockCoroutine == null)
        {
            ResetToDefault();
            serverClockCoroutine = StartCoroutine(Clock());

            Logger.Instance.Log(this, "Start");
        }
    }

    public void StopTimer()
    {
        if (serverClockCoroutine != null)
        {
            StopCoroutine(serverClockCoroutine);
            serverClockCoroutine = null;

            Logger.Instance.Log(this, "Stop");
        }
    }

    public void PrematureStop()
    {
        if (serverClockCoroutine == null)
            return;

        StopTimer();
        OnExpired?.Invoke();
        ResetToDefault();
    }

    public void SetIngredientGuessTime()
    {
        roundTime = ingredientGuessTime;
    }

    public void SetMonsterGuessTime()
    {
        roundTime = monsterGuessTime;
    }

    public void SetPause(bool state)
    {
        isTimePaused = state;
    }
}