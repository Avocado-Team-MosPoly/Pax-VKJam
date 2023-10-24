using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public delegate void StartLoadEvent(string sceneName);
    public static event StartLoadEvent OnLoad;
    public delegate void EndLoadEvent(string sceneName);
    public static event EndLoadEvent EndLoad;
    public static void ServerLoad(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        LoadScene(sceneName);
    }

    public static void Load(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        LoadScene(sceneName);
    }
    private static IEnumerator LoadScene(string sceneName)
    {
        OnLoad?.Invoke(sceneName);
        var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        EndLoad?.Invoke(sceneName);
    }
}