using UnityEngine;

public class CameraPhysics
{
    public static RaycastHit Raycast(Camera camera)
    {
        Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);

        return hitInfo;
    }
}