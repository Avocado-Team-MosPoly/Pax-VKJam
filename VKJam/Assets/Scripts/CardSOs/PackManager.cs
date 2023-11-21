using UnityEngine;
using System.Collections;
public class PackManager : MonoBehaviour
{
    /*public delegate void StartLoadEvent(string sceneName);
    public static event StartLoadEvent OnLoad;*/

    public PackCardSO Active;
    public PackCardSO[] All;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        foreach (var current in All)
        {
            //current.PackDataOwnering();
        }
    }
    /*private void OnApplicationQuit()
    {
        foreach (var current in All)
        {
            current.ResetOwning();
        }
    }*/
}
