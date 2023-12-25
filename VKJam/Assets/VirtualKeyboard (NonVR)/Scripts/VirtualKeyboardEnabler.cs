using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class VirtualKeyboardEnabler : MonoBehaviour
{
    public void ShowVirtualKeyboard()
	{
		VirtualKeyboard.Instance.ShowVirtualKeyboard(GetComponent<TMP_InputField>());
	}
}