using TMPro;
using UnityEngine;

public class LobbyDataInput : MonoBehaviour
{
    [HideInInspector] public string LobbyName { get; private set; }
    [HideInInspector] public int MaxPlayers { get; private set; }
    [HideInInspector] public bool IsTeamMode { get; private set; }
    [HideInInspector] public int RoundAmount { get; private set; }
    [HideInInspector] public Menu_Multiplayer.RecipeMode RecipeMode { get; private set; }

    public static LobbyDataInput Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ChangeLobbyName(string value)
    {
        LobbyName = value;
    }

    public void ChangeMaxPlayers(int value)
    {
        MaxPlayers = value;
    }

    public void ChangeTeamMode(bool value)
    {
        IsTeamMode = value;
    }

    public void ChangeRoundAmount(int value)
    {
        RoundAmount = value;
    }

    public void ChangeRecipeMode(Menu_Multiplayer.RecipeMode value)
    {
        RecipeMode = value;
    }
}