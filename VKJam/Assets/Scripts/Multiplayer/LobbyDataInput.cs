using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyDataInput : MonoBehaviour
{
    [SerializeField] private ButtonSet<int> maxPlayers_ButtonSet;
    [SerializeField] private ButtonSet<bool> teamMode_ButtonSet;
    [SerializeField] private ButtonSet<RecipeMode> RecipeMode_ButtonSet;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TextMeshProUGUI timerText;

public string LobbyJoinCode { get; private set; }
    public string LobbyName { get; private set; } = "";
    public int MaxPlayers { get; private set; } = 2;
    public bool GameMode { get; private set; } = true;
    public int RoundAmount { get; private set; } = 4;
    public float TimerAmount { get; private set; } = 40;
    public RecipeMode RecipeMode { get; private set; } = RecipeMode.Standard;

    public static LobbyDataInput Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        maxPlayers_ButtonSet.OnClick.AddListener(ChangeMaxPlayers);
        teamMode_ButtonSet.OnClick.AddListener(ChangeTeamMode);
        RecipeMode_ButtonSet.OnClick.AddListener(ChangeRecipeMode);
        timerSlider.onValueChanged.AddListener((v) => { timerText.text=v.ToString(); });
        timerSlider.onValueChanged.AddListener(ChangeTimerAmount);
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
        RoundAmount = 2 + value * 2;
    }

    public void ChangeTimerAmount(float value)
    {
        TimerAmount = value;
    }

    public void ChangeRecipeMode(RecipeMode value)
    {
        RecipeMode = value;
    }
}