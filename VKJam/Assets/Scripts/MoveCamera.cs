using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private float Sensitivity;
    [SerializeField] private float TopLimit = -45;
    [SerializeField] private float BotLimit = 45;
    [SerializeField] private float RightLimit = 45;
    [SerializeField] private float LeftLimit = -45;
    // Update is called once per frame
    void Update()
    {
        Debug.Log(transform.rotation);
        if (Input.mousePosition.x >= Screen.width - 5.3f && transform.rotation.y < Mathf.Deg2Rad * RightLimit)
            transform.Rotate(Vector3.up * Sensitivity * Time.deltaTime); 
        if (Input.mousePosition.x <= 0.1f && transform.rotation.y > Mathf.Deg2Rad * LeftLimit)
            transform.Rotate(Vector3.down * Sensitivity * Time.deltaTime);
        if (Input.mousePosition.y >= Screen.height - 2.3f && transform.rotation.x > Mathf.Deg2Rad * TopLimit)
            transform.Rotate(Vector3.left * Sensitivity * Time.deltaTime);
        if (Input.mousePosition.y <= 0.1f && transform.rotation.x < Mathf.Deg2Rad * BotLimit)
            transform.Rotate(Vector3.right * Sensitivity * Time.deltaTime);
        if(transform.rotation.w < 0.975f) transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, 0, 0.975f);
    }
}
