using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public delegate void StartLoadEvent(string sceneName);
    public static event StartLoadEvent OnLoad;
    public delegate void EndLoadEvent(string sceneName);
    public static event EndLoadEvent EndLoad;

    public static void ServerLoad(string sceneName)
    {
        if (!NetworkManager.Singleton.IsServer)
            Logger.Instance.LogWarning("typeof(SceneLoader)", "Server scene loader can use only server");

        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public static void Load(string sceneName)
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
            return;
#endif
        SceneManager.LoadScene(sceneName);
    }

    //private static IEnumerator LoadScene(string sceneName)
    //{
    //    OnLoad?.Invoke(sceneName);
    //    var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
    //    while (!asyncOperation.isDone)
    //    {
    //        yield return null;
    //    }
    //    EndLoad?.Invoke(sceneName);
    //}
}