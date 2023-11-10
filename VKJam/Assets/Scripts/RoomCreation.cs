using UnityEngine;
using TMPro;


public class RoomCreation : MonoBehaviour
{
    private int selectedPlayerCount = 0;
    private bool isTeamMode = false;
    private bool isRecipeModeStandart = false;
    [SerializeField] private TMP_Dropdown cardsDropdown;
    [SerializeField] private TMP_Dropdown roundDropdown;

    public void SelectPlayerCount(int count)
    {
        selectedPlayerCount = count;
    }

    public void SelectGameMode(bool isTeamMode)
    {
        this.isTeamMode = isTeamMode;
    }

    public void SelectRecipeMode(bool isStandart)
    {
        isRecipeModeStandart = isStandart;
    }

    public void SelectDeck()
    {
        int selectedDeckIndex = cardsDropdown.value;
        string selectedDeckName = cardsDropdown.options[selectedDeckIndex].text;
    }

    public void SelectRoundCount()
    {
        int selectedRoundIndex = roundDropdown.value;
        int selectedRoundCount = int.Parse(roundDropdown.options[selectedRoundIndex].text);
    }
}