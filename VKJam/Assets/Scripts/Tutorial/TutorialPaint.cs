using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialPaint : MonoBehaviour
{
    [Serializable]
    private struct TextureSettings
    {
        [Range(2, 1024)] public int sizeX;
        [Range(2, 1024)] public int sizeY;
        public TextureWrapMode wrapMode;
        public FilterMode filterMode;
    }

    [Serializable]
    private enum BrushMode
    {
        Draw,
        Erase
    }

    [SerializeField]
    private TextureSettings textureSettings = new()
    {
        sizeX = 512,
        sizeY = 1024,
        wrapMode = TextureWrapMode.Clamp,
        filterMode = FilterMode.Point
    };
    [SerializeField] private Texture2D texture;

    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Material material;

    [SerializeField] private Camera _camera;
    [SerializeField] private Collider _collider;
    [SerializeField] private Color drawColor;
    [SerializeField] private BrushMode brushMode = BrushMode.Draw;
    [SerializeField] private int brushSize = 12;
    private int halfBrushSize;
    private bool isConnectedToPrevious;

    private Vector2Int previousPoint = new();

    private bool isDraw = false;

    [Header("Controls")]
    [SerializeField, Range(0, 15)] private int scrollRate = 2;
    [Header("Set if functionality is needed")]
    [SerializeField] private Slider brushSizeSlider;
    [SerializeField] private Button switchBrushModeButton;
    [SerializeField] private Button saveAsPNGButton;
    [SerializeField] private Button clearCanvasButton;

    [Header("Sprites")]
    [SerializeField] private Sprite chalkSprite;
    [SerializeField] private Sprite eraserSprite;
    private Image switchBrushModeButtonImage;

    private void Awake()
    {
        baseColor.a = 1f;
        drawColor.a = 1f;

        InitControlUI();
    }

    private void InitControlUI()
    {
        clearCanvasButton?.onClick.AddListener(ClearCanvas);
        saveAsPNGButton?.onClick.AddListener(SavePaintingAsPng);

        switchBrushModeButton?.onClick.AddListener(SwitchBrushMode);
        switchBrushModeButtonImage = switchBrushModeButton?.GetComponent<Image>();

        brushSizeSlider?.onValueChanged.AddListener( ChangeSize );
    }

    private void Start()
    {
        CreateTexture();
        halfBrushSize = brushSize / 2;
    }

    private void CreateTexture()
    {
        if (!material)
        {
            Logger.Instance.LogError(this, "Material isn't set");
            return;
        }
        texture = new Texture2D(textureSettings.sizeX, textureSettings.sizeY);

        texture.wrapMode = textureSettings.wrapMode;
        texture.filterMode = textureSettings.filterMode;

        material.mainTexture = texture;
        Fill(baseColor);
    }

    private void Update()
    {
        Draw();

        int scrollDelta = (int)Input.mouseScrollDelta.y;

        if (scrollDelta != 0)
        {
            brushSize += scrollRate * scrollDelta;
            brushSize = Mathf.Clamp(brushSize, 4, 16);
            halfBrushSize = brushSize / 2;
        }
    }

    private void Draw()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            isDraw = true;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            isConnectedToPrevious = false;
            isDraw = false;
        }

        if (isDraw)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (_collider.Raycast(ray, out RaycastHit hitInfo, 1000f))
            {
                Vector2Int point = new
                (
                    (int)(hitInfo.textureCoord.x * textureSettings.sizeX),
                    (int)(hitInfo.textureCoord.y * textureSettings.sizeY)
                );

                if (isConnectedToPrevious)
                    SmoothDrawCircle(previousPoint, point);
                else
                    DrawCircle(point);

                texture.Apply();

                previousPoint = point;
                isConnectedToPrevious = true;
            }
            else
            {
                isDraw = false;
                isConnectedToPrevious = false;
            }
        }
    }

    private void Fill(Color color)
    {
        for (int x = 0; x < textureSettings.sizeX; x++)
        {
            for (int y = 0; y < textureSettings.sizeY; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
    }

    private void DrawCircle(Vector2Int point)
    {
        float r2 = Mathf.Pow(halfBrushSize - 0.5f, 2);
        Color color = brushMode == BrushMode.Draw ? drawColor : baseColor;

        for (int x = 0; x < brushSize; x++)
        {
            for (int y = 0; y < brushSize; y++)
            {
                float x2 = Mathf.Pow(x - halfBrushSize, 2);
                float y2 = Mathf.Pow(y - halfBrushSize, 2);

                if (x2 + y2 < r2)
                {
                    texture.SetPixel(point.x + x - halfBrushSize, point.y + y - halfBrushSize, color);
                }
            }
        }
    }

    private void SmoothDrawCircle(Vector2Int previousPoint, Vector2Int newPoint)
    {
        Vector2Int currentPoint = new();
        float step = 1f / Vector2Int.Distance(previousPoint, newPoint) / halfBrushSize;

        for (float t = 0; t <= 1f; t += step)
        {
            currentPoint.x = (short)Mathf.Lerp(previousPoint.x, newPoint.x, t);
            currentPoint.y = (short)Mathf.Lerp(previousPoint.y, newPoint.y, t);

            DrawCircle(currentPoint);
        }
    }

    public void SavePaintingAsPng()
    {
        byte[] bytes = texture.EncodeToPNG();
        string dirPath = Application.dataPath + "/Paint Images/";

        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        File.WriteAllBytes($"{dirPath}IMG_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.png", bytes);

        Debug.Log("Path: " + dirPath);
    }

    public void SwitchBrushMode()
    {
        if (brushMode == BrushMode.Draw)
        {
            brushMode = BrushMode.Erase;
            switchBrushModeButtonImage.sprite = eraserSprite;
        }
        else
        {
            brushMode = BrushMode.Draw;
            switchBrushModeButtonImage.sprite = chalkSprite;
        }
    }

    public void ChangeSize(float brushSize)
    {
        int intBrushSize = (int)brushSize;

        this.brushSize = intBrushSize;
        halfBrushSize = intBrushSize / 2;
    }

    public void ClearCanvas()
    {
        Fill(baseColor);
        isConnectedToPrevious = false;
    }

    public void SetActive(bool value)
    {
        enabled = value;
    }
}