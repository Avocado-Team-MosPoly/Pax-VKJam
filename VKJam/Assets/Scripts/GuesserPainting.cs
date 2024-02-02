using UnityEngine;

public class GuesserPainting : MonoBehaviour
{
    [SerializeField] private Material material;

    private Texture2D texture;

    private void OnValidate()
    {
        if (GameManager.Instance.Paint)
        {
            //texture = GameManager.Instance.Paint.GetTexture();

            if (material)
                material.mainTexture = texture;
        }
    }

    private void Start()
    {
        if (!GameManager.Instance.Paint)
        {
            Debug.LogWarning("Paint reference is not set");
            return;
        }
        if (!material)
        {
            Debug.LogWarning("Material reference is not set");
            return;
        }

        //texture = GameManager.Instance.Paint.GetTexture();
        material.mainTexture = texture;
    }
}