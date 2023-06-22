using UnityEngine;

public class GuesserPainting : MonoBehaviour
{
    [SerializeField] private Paint _paint;
    [SerializeField] private Material _material;

    private Texture2D _texture;

    private void OnValidate()
    {
        if (_paint)
        {
            _texture = _paint.GetTexture();

            if (_material)
                _material.mainTexture = _texture;
        }
    }

    private void Start()
    {
        bool isDisable = false;

        if (!_paint)
        {
            Debug.LogWarning("Paint reference is not set");
            isDisable = true;
        }
        if (!_material)
        {
            Debug.LogWarning("Material reference is not set");
            isDisable = true;
        }

        if (isDisable)
            return;

        _texture = _paint.GetTexture();

        if (_material)
            _material.mainTexture = _texture;
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