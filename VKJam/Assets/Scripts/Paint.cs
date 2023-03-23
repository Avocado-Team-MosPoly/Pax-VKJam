using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Paint : MonoBehaviour
{
    [Serializable] private enum BrushMode
    {
        Draw,
        Erase
    }

    [SerializeField, Range(2, 1024)] private int _textureSize = 512;
    [SerializeField] private Color _baseColor;
    [SerializeField] private TextureWrapMode _textureWrapMode;
    [SerializeField] private FilterMode _filterMode;
    [SerializeField] private Texture2D _texture;
    [SerializeField] private Material _material;

    [SerializeField] private Camera _camera;
    [SerializeField] private Collider _collider;
    [SerializeField] private Color _drawColor;
    [SerializeField] private BrushMode _brushMode = BrushMode.Draw;
    [SerializeField] private int _brushSize = 16;
    private int _halfBrushSize;
    private int _prevRayX = -1, _prevRayY;
    private bool _isDraw = false;

    [Header("Просто закинуть ссылку(если не нужен функционал, не ставить)")]
    [SerializeField] private Button clearCanvasButton;
    [SerializeField] private Button saveAsPNGButton;
    [SerializeField] private Button switchBrushButton;
    [SerializeField] private Slider _brushSizeSlider;
    [SerializeField] private TextMeshProUGUI log;

    private void Start()
    {
        if (clearCanvasButton)
            clearCanvasButton.onClick.AddListener(() => Fill(_baseColor));
        if (saveAsPNGButton)
            saveAsPNGButton.onClick.AddListener(SavePaintingAsPng);
        if (switchBrushButton)
            switchBrushButton.onClick.AddListener(SwitchBrush);
        if (_brushSizeSlider)
            _brushSizeSlider.onValueChanged.AddListener(ChangeSize);

        CreateTexture();
        _halfBrushSize = _brushSize / 2;
    }

    private void CreateTexture()
    {
        if (!_material)
        {
            Debug.LogError("Material isn't set");
            return;
        }
        _texture = new Texture2D(_textureSize, _textureSize);
        Fill(_baseColor);

        _texture.wrapMode = _textureWrapMode;
        _texture.filterMode = _filterMode;

        _material.mainTexture = _texture;
        _texture.Apply();
    }

    private void Update()
    {
        Draw();

        if(Input.GetKeyDown(KeyCode.Tab))
        {
            Fill(_baseColor);
        }
    }

    private void Draw()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            _isDraw = true;
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            _prevRayX = -1;
            _isDraw = false;
        }

        if (_isDraw)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (_collider.Raycast(ray, out RaycastHit hitInfo, 1000f))
            {
                int rayX = (int)(hitInfo.textureCoord.x * _textureSize);
                int rayY = (int)(hitInfo.textureCoord.y * _textureSize);

                switch (_brushMode)
                {
                    case BrushMode.Draw:
                        DrawCircle(rayX, rayY, _drawColor);
                        break;
                    case BrushMode.Erase:
                        DrawCircle(rayX, rayY, _baseColor);
                        break;
                }

                if (_prevRayX != -1)
                    SmoothDrawCircle(rayX, rayY);
                
                _prevRayX = rayX;
                _prevRayY = rayY;

                _texture.Apply();
            }
        }
    }

    private void Fill(Color color)
    {
        for (int x = 0; x < _textureSize; x++)
        {
            for (int y = 0; y < _textureSize; y++)
            {
                _texture.SetPixel(x, y, color);
            }
        }
    }
    
    private void DrawCircle(int rayX, int rayY, Color color)
    {
        for (int x = 0; x < _brushSize; x++)
        {
            for (int y = 0; y < _brushSize; y++)
            {
                float x2 = Mathf.Pow(x - _halfBrushSize, 2);
                float y2 = Mathf.Pow(y - _halfBrushSize, 2);
                float r2 = Mathf.Pow(_halfBrushSize - 0.5f, 2);

                if (x2 + y2 < r2)
                {
                    _texture.SetPixel(rayX + x - _halfBrushSize, rayY + y - _halfBrushSize, color);
                }
            }
        }
    }

    private void DrawSquare(int rayX, int rayY)
    {
        for (int x = 0; x < _brushSize; x++)
        {
            for (int y = 0; y < _brushSize; y++)
            {
                _texture.SetPixel(rayX + x - _halfBrushSize, rayY + y - _halfBrushSize, _drawColor);
            }
        }
    }

    private void SmoothDrawCircle(int rayX, int rayY)
    {
        Vector2Int prevPoint = new Vector2Int(_prevRayX, _prevRayY);
        Vector2Int newPoint = new Vector2Int(rayX, rayY);
        Vector2Int currentPoint = new Vector2Int();
        float step = 1f / Vector2Int.Distance(prevPoint, newPoint) / _halfBrushSize;

        for (float t = 0; t <= 1f; t += step)
        {
            currentPoint.x = (int)Mathf.Lerp(prevPoint.x, newPoint.x, t);
            currentPoint.y = (int)Mathf.Lerp(prevPoint.y, newPoint.y, t);
            DrawCircle(currentPoint.x, currentPoint.y, _drawColor);
        }
    }

    public void SavePaintingAsPng()
    {
        byte[] bytes = _texture.EncodeToPNG();
        string dirPath = Application.dataPath + "/Paint Images/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes($"{dirPath}IMG_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.png", bytes);
        Debug.Log("Path: " + dirPath);
    }

    public void SwitchBrush()
    {
        if (_brushMode == BrushMode.Draw)
            _brushMode = BrushMode.Erase;
        else
            _brushMode = BrushMode.Draw;
    }

    public void ChangeSize(float value)
    {
        _brushSize = Mathf.RoundToInt(value * _textureSize);
    }

    public Texture2D GetTexture()
    {
        return _texture;
    }
}