using UnityEngine;
using TMPro;

public class FPSManager : MonoBehaviour
{
    public float CurrentFPS { get; private set; }

    [SerializeField] private TextMeshProUGUI fpsLabel;

    private void Update()
    {
        CurrentFPS = 1f / Time.deltaTime;

        if (fpsLabel != null)
            fpsLabel.text = ((int)CurrentFPS).ToString();
    }

    public void SetTargetFPS(string value)
    {
        if (string.IsNullOrEmpty(value))
            Application.targetFrameRate = 60;
        else if (int.TryParse(value, out int targetFPS))
            Application.targetFrameRate = targetFPS;
        else
            Debug.LogError(new System.FormatException());
    }
    public void SetTargetFPS(int value)
    {
        Application.targetFrameRate = value;
    }
}
