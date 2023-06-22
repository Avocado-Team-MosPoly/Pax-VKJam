using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static void ServerLoad(string sceneName)
    {
        //SceneManager.LoadScene(sceneName);
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public static void Load(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}