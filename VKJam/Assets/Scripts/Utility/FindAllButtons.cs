using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FindAllButtons : MonoBehaviour
{
    [SerializeField] private SoundList soundList;
    [SerializeField] private List<Button> buttons;

    private void Start()
    {
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => soundList.Play("button-click"));
        }
    }

    [ContextMenu("Find all buttons on scene")]
    private void FindAllButtonsOnScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        List<GameObject> objectsInScene = new List<GameObject>();

        for (int i = 0; i < rootObjects.Length; i++)
        {
            objectsInScene.Add(rootObjects[i]);
        }

        buttons.Clear();

        for (int i = 0; i < allObjects.Length; i++)
        {
            if (allObjects[i].transform.root)
            {
                for (int i2 = 0; i2 < rootObjects.Length; i2++)
                {
                    if (allObjects[i].transform.root == rootObjects[i2].transform && allObjects[i] != rootObjects[i2])
                    {
                        if (allObjects[i].TryGetComponent<Button>(out Button button))
                        {
                            buttons.Add(button);
                        }
                        break;
                    }
                }
            }
        }
    }
}