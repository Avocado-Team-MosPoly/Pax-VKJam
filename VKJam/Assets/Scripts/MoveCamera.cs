using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private float YminClamp, YmaxClamp;
    [SerializeField] private float XminClamp, XmaxClamp;

    public float mouseSensitivity = 100f;

    private Vector2 Rotation;

    private void OnEnable()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * -1;
        Rotation.y += mouseX;
        Rotation.y = Mathf.Clamp(Rotation.y, YminClamp, YmaxClamp);
        Rotation.x += mouseY;
        Rotation.x = Mathf.Clamp(Rotation.x, XminClamp, XmaxClamp);

        transform.localRotation = Quaternion.Euler(Rotation.x, Rotation.y, 0f);
    }
}