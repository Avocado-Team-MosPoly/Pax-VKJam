using System.Collections.Generic;
using UnityEngine;

public class CameraPhysics
{
    private static readonly Dictionary<Camera, (Vector3, GameObject)> dictionary = new();

    public static RaycastHit Raycast(Camera camera)
    {
        Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);

        return hitInfo;
    }

    private static bool IsCalculatedOnMousePosition(Camera camera)
    {
        if (!dictionary.ContainsKey(camera))
            return false;

        if (dictionary[camera].Item1 != Input.mousePosition)
            return false;

        return true;
    }
}