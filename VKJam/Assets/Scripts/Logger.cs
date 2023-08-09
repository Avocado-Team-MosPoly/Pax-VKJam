using TMPro;
using UnityEngine;

public class Logger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI output;

    public static Logger Instance {  get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance.gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    public void Log(object message)
    {
        output.text += message + "\n";
    }
}