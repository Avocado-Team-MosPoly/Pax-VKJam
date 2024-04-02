using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class VirtualKeyboard : MonoBehaviour
{
	public static VirtualKeyboard Instance {  get; private set; }

	[SerializeField] private GameObject canvas;
    [SerializeField] private TextMeshProUGUI inputTextView;

    private TMP_InputField targetText;
	private Coroutine deletionCoroutine;

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

	private IEnumerator StartDeleteCoroutine()
	{
		int deletionCountToClear = 15;
        float timeBtwDeletionsInSeconds = 0.2f;
        float timeBtwDeletionsInSecondsDecreaser = 0.02f;

        for (int i = 0; i < deletionCountToClear; i++)
		{
			if (Del())
			{
                yield return new WaitForSeconds(timeBtwDeletionsInSeconds);
				timeBtwDeletionsInSeconds -= timeBtwDeletionsInSecondsDecreaser;
			}
			else
				yield break;

			if (i == deletionCountToClear - 1)
				Clear();
		}
	}

	public void Clear()
	{
        targetText.text = string.Empty;
        inputTextView.text = string.Empty;
    }

    public bool Del()
	{
		if (targetText.text.Length <= 0)
			return false;

		targetText.text = targetText.text.Remove(targetText.text.Length - 1, 1);
		inputTextView.text = targetText.text;

		return true;
    }

	public void StartDelete()
	{
        if (deletionCoroutine != null || targetText.text.Length <= 0)
            return;

		deletionCoroutine = StartCoroutine(StartDeleteCoroutine());
    }

	public void StopDelete()
	{
		if (deletionCoroutine != null)
		{
			StopCoroutine(deletionCoroutine);
			deletionCoroutine = null;
		}
	}

	public void PasteFromClipboard()
	{
		if (!string.IsNullOrEmpty(GUIUtility.systemCopyBuffer))
		{
			targetText.text = GUIUtility.systemCopyBuffer;
			inputTextView.text = targetText.text;
        }
    }

	public bool ShowVirtualKeyboard(TMP_InputField targetText)
	{
		if (targetText == null)
			return false;

#if UNITY_WEBGL && !UNITY_EDITOR
        if (Application.isMobilePlatform)
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