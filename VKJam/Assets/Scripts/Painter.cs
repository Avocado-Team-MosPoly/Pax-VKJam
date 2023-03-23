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
        // Выбор ингредиента
    }

    private static void LoseRound()
    {
        TokensManager.AddScore(-1);
        
        if (TokensManager.TokensCount <= 0)
            LoseGame();
        else
            // Сообщить всем игрокам о проигрыше
            NextPainter();
    }

    private static void LoseGame()
    {
        // Сообщить всем игрокам о конце игры
    }

    public static void WinRound()
    {
        TokensManager.AddScore(1);

        if (TokensManager.TokensCount <= 0)
            WinGame();
        else
            // Сообщить всем игрокам об отгадывании
            NextPainter();
    }

    private static void WinGame()
    {
        // Сообщить всем игрокам о выигрыше
    }

    private static void NextPainter()
    {
        // Переход к следующему ингредиенту
    }

    /// <summary> Временно </summary>
    public static bool CompareAnswer(string guess)
    {
        return guess != answer;
    }
}