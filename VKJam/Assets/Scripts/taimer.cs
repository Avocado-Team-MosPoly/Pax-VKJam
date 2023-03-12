using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class taimer : MonoBehaviour
{
    [SerializeField] private TMP_Text ShowTime;
    [SerializeField] private int time;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Clock());
    }

    IEnumerator Clock()
    {
        while (true)
        {
            time -= 1;
            if (time / 60 <= 10)
            {
                ShowTime.text = "0" + time / 60 + ":" + time % 60;
                if (time % 60 < 10)
                {
                    ShowTime.text = "0" + time / 60 + ":" + "0" + time % 60;
                }
            }
            else
            {
                ShowTime.text = time / 60 + ":" + time % 60;
                if (time % 60 < 10)
                {
                    ShowTime.text = time / 60 + ":" + "0" + time % 60;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
}
