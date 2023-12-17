using UnityEngine;

public class GuesserPainting : MonoBehaviour
{
    [SerializeField] private Paint paint;
    [SerializeField] private Material material;

    private Texture2D texture;

    private void OnValidate()
    {
        if (paint)
        {
            texture = paint.GetTexture();

            if (material)
                material.mainTexture = texture;
        }
    }

    private void Start()
    {
        if (!paint)
        {
            Debug.LogWarning("Paint reference is not set");
            return;
        }
        if (!material)
        {
            Debug.LogWarning("Material reference is not set");
            return;
        }

        texture = paint.GetTexture();
        material.mainTexture = texture;
    }

    //private void Update()
    //{
    //    if (_paint)
    //    {
    //        _texture = _paint.GetTexture();

    //        if (_material)
    //            _material.mainTexture = _texture;
    //    }
    //}
}