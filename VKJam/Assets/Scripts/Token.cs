using UnityEngine;

public class Token : MonoBehaviour
{
    public void OnSpawn()
    {
        // ��� ���������� ��� ������
    }

    public void OnDestruct()
    {
        // ��� ���������� ��� �����������
        Destroy(gameObject);
    }
}