using TMPro;
using UnityEngine;

public class LobbyDataInput : MonoBehaviour
{
    public string LobbyJoinCode { get; private set; }
    public string LobbyName { get; private set; }
    public int MaxPlayers { get; private set; } = 2;
    public bool GameMode { get; private set; } = true;
    public int RoundAmount { get; private set; } = 4;
    public RecipeMode RecipeMode { get; private set; } = RecipeMode.Standard;

    public static LobbyDataInput Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ChangeLobbyCode(string value)
    {
        LobbyJoinCode = value;
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
        GameMode = value; 
    }

    public void ChangeRoundAmount(int value)
    {
        RoundAmount = value;
    }

    public void ChangeRecipeMode(RecipeMode value)
    {
        RecipeMode = value;
    }
}