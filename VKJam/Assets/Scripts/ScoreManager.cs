using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text Fishka;
    [SerializeField] private GameObject PrefabFishki;
    [SerializeField] private GameObject SpawnFishki;
    private static TMP_Text fishkaText;
    [SerializeField] static private GameObject prefabFishki;
    [SerializeField] static private GameObject spawnFishki;
    public static int Score { get; private set; }
    private static List<GameObject> allFishki;

    private void Start()
    {
        fishkaText = Fishka;
    }
    public static void AddScore(int value)
    {
        Score += value;
        fishkaText.text = Score.ToString() + "X";
        allFishki.Add(Instantiate(prefabFishki, spawnFishki.transform.position, spawnFishki.transform.rotation, spawnFishki.transform));
    }
    public static void RemoveScore(int value)
    {
        Score -= value;
        if (Score<0)
        {
            Score = 0;
        }
        fishkaText.text = Score.ToString() + "X";
        Destroy(allFishki[allFishki.Count - 1]);
        allFishki.RemoveAt(allFishki.Count - 1);
    }
}