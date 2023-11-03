using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.Events;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class Timer : NetworkBehaviour
{
    [SerializeField] private TMP_Text ShowTime;
    public NetworkVariable<int> NetworkTime = new(0);
    private bool isTimePaused = false;
    [SerializeField] private Hint showRecepiesUI;

    [SerializeField] private int ingredientGuessTime = 45;
    [SerializeField] private int monsterGuessTime = 120;

    private int roundTime;

    private Coroutine serverClockCoroutine = null;

    [HideInInspector] public UnityEvent OnExpired;

    public static Timer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        roundTime = ingredientGuessTime;//int.Parse(LobbyManager.Instance.CurrentLobby.Data["TimerAmount"].Value);
        ShowTime.text = ToTimeFormat(roundTime);
    }

    public override void OnNetworkSpawn()
    {
        NetworkTime.OnValueChanged += OnTimerChange;

        if (IsServer)
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
                StopServerRpc();

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

    [ServerRpc]
    public void StartServerRpc()
    {
        Debug.Log("Start Server RPC");

        ResetToDefault();

        if (serverClockCoroutine == null)
            serverClockCoroutine = StartCoroutine(Clock());

        //SetPause(false);
    }

    [ServerRpc]
    public void StopServerRpc()
    {
        Debug.Log("Stop Server RPC");

        if (serverClockCoroutine != null)
        {
            StopCoroutine(serverClockCoroutine);
            serverClockCoroutine = null;
        }
    }
    public void OnIngredientGuess()
    {
        roundTime = ingredientGuessTime;
    }

    public void OnMonsterGuess()
    {
        roundTime = monsterGuessTime;
    }

    public void SetPause(bool state)
    {
        Debug.Log("Set Pause: " + state);
        isTimePaused = state;
    }
}