using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTracker : MonoBehaviour
{
    [SerializeField] private float offset;
    private Vector3 difference;
    private float rotateZ;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        difference = cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        rotateZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotateZ + offset);
        //transform.rotation = Quaternion.LookRotation(difference);
    }
}
