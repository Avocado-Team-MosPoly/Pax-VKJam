using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Unity.VisualScripting;

public class taimer : MonoBehaviour
{
    [SerializeField] private TMP_Text ShowTime;
    [SerializeField] private int time;
    [SerializeField] private ShowRecepiesUI showRecepiesUI;
    private int reloadtime;

    private void Start()
    {
        reloadtime = time;
    }
    private void OnEnable() 
    {
        StartCoroutine(Clock());
    }

    IEnumerator Clock()
    {
        while (true)
        {
            if (time <= 0)
            {
                transform.parent.gameObject.SetActive(false);
                showRecepiesUI.Hide();
                time = reloadtime + 3;
            }

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
