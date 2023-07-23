using UnityEngine;

public class Painter : MonoBehaviour
{
    [SerializeField] private Paint paint;
    [SerializeField] private GameObject mainCards;
    [SerializeField] private GameObject[] gameObjects;

    [SerializeField] private CardManager cardManager;

    public void Activate()
    {
        foreach (GameObject obj in gameObjects)
            obj.SetActive(true);

        mainCards.SetActive(true);
        paint.ClearCanvas(); // плохо работает
        paint.SetMode(true);
    }

    public void Deactivate()
    {
        foreach (GameObject obj in gameObjects)
            obj.SetActive(false);

        paint.SetMode(false);
        cardManager.enabled = false;
    }
}