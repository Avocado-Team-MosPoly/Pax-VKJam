using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneOpeningHelper
{
    [MenuItem("Scenes/Bootstrap")]
    public static void OpenBootstrapScene()
    {
        OpenScene("Assets/Scenes/MainScenes/BootScene.unity");
    }

    [MenuItem("Scenes/Second Mode")]
    public static void OpenSecondModeScene()
    {
        OpenScene("Assets/Scenes/InProgress/SecondModeDrawing.unity");
    }

    public static void OpenScene(string sceneName)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(sceneName);
    }
}