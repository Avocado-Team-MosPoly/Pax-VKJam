using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneOpeningHelper
{
    [MenuItem("Scenes/Bootstrap", priority = 0)]
    public static void OpenBootstrapScene()
    {
        OpenScene("Assets/Scenes/MainScenes/BootScene.unity");
    }

    [MenuItem("Scenes/Menu", priority = 1)]
    public static void OpenMenuScene()
    {
        OpenScene("Assets/Scenes/MainScenes/Menu.unity");
    }
    
    [MenuItem("Scenes/Main Mode", priority = 2)]
    public static void OpenMainGameScene()
    {
        OpenScene("Assets/Scenes/MainScenes/Map_New.unity");
    }
    
    [MenuItem("Scenes/Second Mode", priority = 3)]
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