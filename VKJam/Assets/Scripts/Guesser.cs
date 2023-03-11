using UnityEngine;

public class Guesser : MonoBehaviour
{
    public string guess;

    public void ChangeGuess(string value)
    {
        guess = value;
    }

    public void SubmitGuess()
    {
        if (Painter.CompareAnswer(guess))
        {
            Debug.Log("Wrong guess");
            Painter.WinRound();
            // �������� ���� ������� � ���������� ������
        }
        else
        {
            Debug.Log("Correct guess");
            // ������������ �����
        }
    }
}