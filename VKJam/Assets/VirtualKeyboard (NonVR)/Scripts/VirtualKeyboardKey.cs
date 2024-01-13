using UnityEngine;

public class VirtualKeyboardKey : MonoBehaviour
{
    [SerializeField] private string keyValue = string.Empty;

    public void KeyClick()
    {
        VirtualKeyboard.Instance.KeyPress(keyValue);
    }
}