using UnityEngine;

public class Token : MonoBehaviour
{
    private void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        // ��� ���������� ��� ������
    }

    public void Destruct()
    {
        // ��� ���������� ��� �����������
        Destroy(gameObject);
    }
}