using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Paint_new : MonoBehaviour
{
    public enum BrushMode
    {
        Draw,
        Erase
    }

    [Header("Texture Options")]
    [Space(3)]

    [SerializeField, Range(256, 1024)] private int textureSize = 512;
    [SerializeField] private TextureWrapMode textureWrapMode;
    [SerializeField] private FilterMode filterMode;
    
    [SerializeField] private Color baseColor;
    
    [Space(6)]
    [SerializeField] private Material material;
    
    private Texture2D texture;

    [Header("Brush Options")]
    [Space(3)]

    [SerializeField] private BrushMode brushMode = BrushMode.Draw;
    [SerializeField] private int brushSize = 16;
    
    [SerializeField] private Color drawColor;
    
    private int halfBrushSize;
    private int prevRayX = -1, prevRayY;
    private bool isHoldClick = false;

    [Header("Click Track Options")]
    [Space(3)]

    [SerializeField] private Camera _camera;
    [SerializeField] private Collider _collider;

    [Header("Просто закинуть ссылку(если не нужен функционал, не ставить)")]
    [Space(3)]
    
    [SerializeField] private Button clearCanvasButton;
    [SerializeField] private Button saveAsPNGButton;
    [SerializeField] private Button switchBrushButton;
    [SerializeField] private Slider brushSizeSlider;
    [SerializeField] private TextMeshProUGUI log;

    private void Start()
    {
        if (clearCanvasButton)
            clearCanvasButton.onClick.AddListener(() => Fill(baseColor));
        if (saveAsPNGButton)
            saveAsPNGButton.onClick.AddListener(SavePaintingAsPng);
        if (switchBrushButton)
            //switchBrushButton.onClick.AddListener(SwitchBrushMode);
        if (brushSizeSlider)
            brushSizeSlider.onValueChanged.AddListener(ChangeSize);

        halfBrushSize = brushSize / 2;

        CreateTexture();
    }

    private void CreateTexture()
    {
        if (!material)
        {
            Debug.LogError("Material isn't set");
            return;
        }

        texture = new Texture2D(textureSize, textureSize);
        Fill(baseColor);

        texture.wrapMode = textureWrapMode;
        texture.filterMode = filterMode;

        material.mainTexture = texture;
        texture.Apply();
    }

    private void Update()
    {
        Draw();
    }

    private void Draw()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            isHoldClick = true;

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            prevRayX = -1;
            isHoldClick = false;
        }

        if (isHoldClick)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (_collider.Raycast(ray, out RaycastHit hitInfo, 1000f))
            {
                int rayX = (int)(hitInfo.textureCoord.x * textureSize);
                int rayY = (int)(hitInfo.textureCoord.y * textureSize);

                switch (brushMode)
                {
                    case BrushMode.Draw:
                        DrawCircle(rayX, rayY, drawColor);
                        break;
                    case BrushMode.Erase:
                        DrawCircle(rayX, rayY, baseColor);
                        break;
                }

                if (prevRayX != -1)
                    SmoothDrawCircle(rayX, rayY);

                prevRayX = rayX;
                prevRayY = rayY;

                texture.Apply();
            }
        }
    }

    private void Fill(Color color)
    {
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }

    private void DrawCircle(int rayX, int rayY, Color color)
    {
        for (int x = 0; x < brushSize; x++)
        {
            for (int y = 0; y < brushSize; y++)
            {
                float x2 = Mathf.Pow(x - halfBrushSize, 2);
                float y2 = Mathf.Pow(y - halfBrushSize, 2);
                float r2 = Mathf.Pow(halfBrushSize - 0.5f, 2);

                if (x2 + y2 < r2)
                {
                    texture.SetPixel(rayX + x - halfBrushSize, rayY + y - halfBrushSize, color);
                }
            }
        }
    }

    private void SmoothDrawCircle(int rayX, int rayY)
    {
        Vector2Int prevPoint = new Vector2Int(prevRayX, prevRayY);
        Vector2Int newPoint = new Vector2Int(rayX, rayY);
        Vector2Int currentPoint = new Vector2Int();
        float step = 1f / Vector2Int.Distance(prevPoint, newPoint) / halfBrushSize;

        for (float t = 0; t <= 1f; t += step)
        {
            currentPoint.x = (int)Mathf.Lerp(prevPoint.x, newPoint.x, t);
            currentPoint.y = (int)Mathf.Lerp(prevPoint.y, newPoint.y, t);

            switch (brushMode)
            {
                case BrushMode.Draw:
                    DrawCircle(currentPoint.x, currentPoint.y, drawColor);
                    break;
                case BrushMode.Erase:
                    DrawCircle(currentPoint.x, currentPoint.y, baseColor);
                    break;
            }
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

    public void SwitchBrushMode(BrushMode newBrushMode)
    {
        brushMode = newBrushMode;
    }

    public void ChangeSize(float value)
    {
        brushSize = Mathf.RoundToInt(value * textureSize);
        halfBrushSize = brushSize / 2;
    }
}