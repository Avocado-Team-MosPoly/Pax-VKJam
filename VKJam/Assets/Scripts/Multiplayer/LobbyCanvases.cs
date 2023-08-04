using UnityEngine;

public class LobbyCanvases : MonoBehaviour
{
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Canvas settingsCanvas;

    private Canvas currentCanvas;

    private void Awake()
    {
        currentCanvas = mainCanvas;
    }

    public void ToMain()
    {
        currentCanvas.gameObject.SetActive(false);
        currentCanvas = mainCanvas;
        currentCanvas.gameObject.SetActive(true);
    }

    public void ToSettings()
    {
        currentCanvas.gameObject.SetActive(false);
        currentCanvas = settingsCanvas;
        currentCanvas.gameObject.SetActive(true);
    }
}