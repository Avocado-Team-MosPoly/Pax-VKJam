using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SetUpAllButtons : MonoBehaviour
{
    [Header("Search Options")]
    [SerializeField] private List<string> ignoreNames;

    [Header("Search Results")]
    [SerializeField] private List<Button> buttons;

    private void Start()
    {
        SoundList buttonSoundList = BackgroundMusic.Instance.GetComponentInChildren<SoundList>();
        UnityAction onClick = () => buttonSoundList.Play("button-click");

        foreach (Button button in buttons)
        {
            button.onClick.AddListener(onClick);
        }
    }

    [ContextMenu("Find all buttons on scene")]
    private void FindAllButtonsOnScene()
    {
#if UNITY_EDITOR
        Scene activeScene = SceneManager.GetActiveScene();

        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        List<GameObject> objectsInScene = new();

        for (int i = 0; i < rootObjects.Length; i++)
            objectsInScene.Add(rootObjects[i]);

        buttons = buttons == null ? new() : new(buttons.Count);

        for (int i = 0; i < ignoreNames.Count; i++)
        {
            if (string.IsNullOrEmpty(ignoreNames[i]))
                ignoreNames.RemoveAt(i--);
            else
                ignoreNames[i] = ignoreNames[i].ToLower();
        }

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
                            string buttonName = button.gameObject.name.ToLower();
                            bool flag = true;

                            foreach (string ignore in ignoreNames)
                            {
                                if (buttonName.Contains(ignore))
                                {
                                    flag = false;
                                    break;
                                }
                            }

                            if (flag)
                                buttons.Add(button);
                        }

                        break;
                    }
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);
#endif
    }
}