using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTracker : MonoBehaviour
{
    public float offset;
    private Vector3 difference;
    private float rotateZ;
    private void Update()
    {
        difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        rotateZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f,0f, rotateZ + offset);
    }
}
