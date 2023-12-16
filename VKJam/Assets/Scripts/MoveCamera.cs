using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private float YminClamp, YmaxClamp;
    [SerializeField] private float XminClamp, XmaxClamp;
    public bool ActiveMove = true;

    public float mouseSensitivity = 20f;

    private Vector2 Rotation;

    public void SetActivity(bool Target)
    {
        ActiveMove = Target;
        if (Target == false) //transform.localRotation = Quaternion.Euler((XminClamp + XmaxClamp) / 2, (YminClamp + YmaxClamp) / 2, 0f);
            transform.localEulerAngles = new Vector3((XmaxClamp + XminClamp) / 2, (YmaxClamp + YminClamp) / 2, 0f);
    }

    private void Update()
    {
        if (!ActiveMove)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * -1;

        Rotation += new Vector2(mouseY, mouseX);
        Rotation.x = Mathf.Clamp(Rotation.x, XminClamp, XmaxClamp);
        Rotation.y = Mathf.Clamp(Rotation.y, YminClamp, YmaxClamp);

        transform.localRotation = Quaternion.Euler(Rotation);
    }
}