using TMPro;
using UnityEngine;

public class Logger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI output;

    public static Logger Instance {  get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Log(object message)
    {
        output.text += message + "\n";
    }
}