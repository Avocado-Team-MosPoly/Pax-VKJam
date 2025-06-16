using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyDataInput : MonoBehaviour
{
    [SerializeField] private ButtonSet<int> maxPlayers_ButtonSet_MainGame;
    [SerializeField] private Slider maxPlayers_Slider_SecondMode;
    [SerializeField] private ButtonSet<bool> teamMode_ButtonSet;
    [SerializeField] private ButtonSet<bool> privacy_ButtonSet;
    [SerializeField] private TMP_Dropdown region;

    [SerializeField] private Button toMainGameSettings;
    [SerializeField] private Button toSecondModeSettings;
    //[SerializeField] private ButtonSet<RecipeMode> RecipeMode_ButtonSet;
    //[SerializeField] private Slider timerSlider;
    //[SerializeField] private TextMeshProUGUI timerText;

    public bool IsSecondMode { get; private set; }
    public string LobbyJoinCode { get; private set; }
    public string LobbyName { get; private set; } = "";
    public int MaxPlayers { get; private set; } = 2;
    public bool GameMode { get; private set; } = true;
    public int RoundAmount { get; private set; } = 4;
    public float TimerAmount { get; private set; } = 45;
    public RecipeMode RecipeMode { get; private set; } = RecipeMode.Standard;
    public bool IsPrivate { get; private set; } = false;

    public static LobbyDataInput Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        region.onValueChanged.AddListener(ChangeRegion);
        maxPlayers_ButtonSet_MainGame.OnClick.AddListener(ChangeMaxPlayers);
        maxPlayers_Slider_SecondMode.onValueChanged.AddListener(v => ChangeMaxPlayers((int)v));
        teamMode_ButtonSet.OnClick.AddListener(ChangeTeamMode);
        privacy_ButtonSet.OnClick.AddListener(ChangePrivacy);
        
        toMainGameSettings.onClick.AddListener(() => IsSecondMode = false);
        toSecondModeSettings.onClick.AddListener(() => IsSecondMode = true);
        
        //RecipeMode_ButtonSet.OnClick.AddListener(ChangeRecipeMode);
        //timerSlider.onValueChanged.AddListener((v) => { timerText.text=v.ToString(); });
        //timerSlider.onValueChanged.AddListener(ChangeTimerAmount);
    }

    public void ChangeRegion(int value)
    {
        RelayManager.Instance.ChangeRegion(region.options[value].text);
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

    public void ChangePrivacy(bool isPrivate)
    {
        IsPrivate = isPrivate;
    }
}