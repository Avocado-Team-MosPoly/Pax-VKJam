using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class VirtualKeyboard : MonoBehaviour
{
	public static VirtualKeyboard Instance {  get; private set; }

	[SerializeField] private GameObject canvas;
    [SerializeField] private TextMeshProUGUI inputTextView;

    private TMP_InputField targetText;

    [DllImport("__Internal")] private static extern bool UnityPluginIsMobilePlatform();

    private void Start()
    {
		if (Instance == null)
			Instance = this;
		else
		{
			Logger.Instance.LogWarning(this, $"Two or more {nameof(VirtualKeyboard)} on scene: {nameof(Instance)} - {Instance.gameObject.name}, duplicate - {gameObject.name}");
			Destroy(gameObject);
		}

		HideVirtualKeyboard();
    }

	public void KeyPress(string k)
	{
		targetText.text += k;
        inputTextView.text = targetText.text;
    }

    public void Del()
	{
		if (targetText.text.Length <= 0)
			return;

		targetText.text = targetText.text.Remove(targetText.text.Length - 1, 1);
		inputTextView.text = targetText.text;
    }
	
	public bool ShowVirtualKeyboard(TMP_InputField targetText)
	{
		if (targetText == null)
			return false;

#if UNITY_WEBGL && !UNITY_EDITOR
        if (UnityPluginIsMobilePlatform() || Application.isMobilePlatform)
        {
			this.targetText = targetText;
			inputTextView.text = targetText.text;
			canvas.SetActive(true);
			return true;
        }
#endif

		return false;
	}
	
	public void HideVirtualKeyboard()
	{
		canvas.SetActive(false);
    }
}