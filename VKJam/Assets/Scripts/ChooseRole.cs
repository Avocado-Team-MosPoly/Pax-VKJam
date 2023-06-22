using Unity.Netcode;
using UnityEngine;

public class ChooseRole : NetworkBehaviour
{
    public GameObject[] painter;
    public GameObject[] guesser;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Server");
            SetPainter();
        }
        else
        {
            Debug.Log("Client");
            SetGuesser();
        }
    }

    private void SetPainter()
    {
        for (int i = 0; i < painter.Length; i++)
        {
            painter[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < guesser.Length; i++)
        {
            guesser[i].gameObject.SetActive(false);
        }
    }

    private void SetGuesser()
    {
        for (int i = 0; i < painter.Length; i++)
        {
            painter[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < guesser.Length; i++)
        {
            guesser[i].gameObject.SetActive(true);
        }
    }
}