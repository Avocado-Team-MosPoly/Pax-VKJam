using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text Fishka;
    private static TMP_Text fishka;
    public static int Score { get; private set; }

    private void Start()
    {
        fishka = Fishka;
    }
    public static void AddScore(int value)
    {
        Score += value;
        fishka.text = Score.ToString() + "X";
    }
    public static void RemoveScore(int value)
    {
        Score -= value;
        if (Score<0)
        {
            Score = 0;
        }
        fishka.text = Score.ToString() + "X";
    }
}