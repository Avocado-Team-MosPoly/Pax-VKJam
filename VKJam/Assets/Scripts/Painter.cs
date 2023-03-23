using Unity.VisualScripting;
using UnityEngine;

public class Painter : MonoBehaviour
{
    private static string answer = "Carrot";
    private static float timer;

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
            LoseRound();
    }

    public static void ChooseAnswer()
    {
        // ����� �����������
    }

    private static void LoseRound()
    {
        TokensManager.AddScore(-1);
        
        if (TokensManager.TokensCount <= 0)
            LoseGame();
        else
            // �������� ���� ������� � ���������
            NextPainter();
    }

    private static void LoseGame()
    {
        // �������� ���� ������� � ����� ����
    }

    public static void WinRound()
    {
        TokensManager.AddScore(1);

        if (TokensManager.TokensCount <= 0)
            WinGame();
        else
            // �������� ���� ������� �� �����������
            NextPainter();
    }

    private static void WinGame()
    {
        // �������� ���� ������� � ��������
    }

    private static void NextPainter()
    {
        // ������� � ���������� �����������
    }

    /// <summary> �������� </summary>
    public static bool CompareAnswer(string guess)
    {
        return guess != answer;
    }
}