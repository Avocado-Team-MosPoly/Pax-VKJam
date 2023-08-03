using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GuessHistory : NetworkBehaviour
{
    [SerializeField] private int VisibleGuessesCount = 4;

    [SerializeField] private TextMeshProUGUI TMProUI;

    private Dictionary<short, List<string>> guessHistory = new();

    private string[] VisibleGuesses;
    private int currentGuessIndex;

    public static GuessHistory Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        VisibleGuesses = new string[VisibleGuessesCount];
    }

    private void UpdateUI()
    {
        Debug.Log("[GuessHistory] UpdateUI");

        TMProUI.text = string.Empty;

        foreach (string guess in VisibleGuesses)
            TMProUI.text += guess + "\n";
    }

    [ClientRpc]
    private void AddGuessClientRpc(short clientId, FixedString32Bytes guess)
    {
        if (guessHistory.ContainsKey(clientId))
            guessHistory[clientId].Add(guess.ToString());
        else
            guessHistory[clientId] = new() { guess.ToString() };

        if (currentGuessIndex < VisibleGuessesCount)
        {
            VisibleGuesses[currentGuessIndex] = guess.ToString();
            currentGuessIndex++;
        }
        else
        {
            for (int i = 0; i < VisibleGuessesCount - 1; i++)
                VisibleGuesses[i] = VisibleGuesses[i + 1];

            VisibleGuesses[VisibleGuessesCount - 1] = guess.ToString();
        }

        UpdateUI();
    }

    public void AddGuess(ulong clientId, string guess)
    {
        Debug.Log("[GuessHistory] AddGuess");
        AddGuessClientRpc((short)clientId, guess);
    }
}