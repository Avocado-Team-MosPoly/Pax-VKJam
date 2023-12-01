using System;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Paint : NetworkBehaviour
{
    [Serializable]
    private struct TextureSettings
    {
        [Range(2, 2048)] public int sizeX;
        [Range(2, 2048)] public int sizeY;
        public TextureWrapMode wrapMode;
        public FilterMode filterMode;
    }
    private struct Vector2Short : INetworkSerializable
    {
        public short x;
        public short y;

        public Vector2Short(int x, int y)
        {
            this.x = (short)x;
            this.y = (short)y;
        }
        public Vector2Short(short x, short y)
        {
            this.x = x;
            this.y = y;
        }

        public static float Distance(Vector2Short a, Vector2Short b)
        {
            Vector2Short direction = b - a;
            float distance = Mathf.Sqrt(direction.x * direction.x + direction.y * direction.y);
            
            return distance;
        }

        public static Vector2Short operator - (Vector2Short a, Vector2Short b)
        {
            return new Vector2Short
            {
                x = (short)(b.x - a.x),
                y = (short)(b.y - a.y)
            };
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref y);
        }

        public override string ToString()
        {
            return $"({nameof(Vector2Short)}({x}, {y})";
        }
    }
    private struct DrawingParams : INetworkSerializable
    {
        public short prevX;
        public short prevY;
        public short newX;
        public short newY;
        public byte brushSize;

        public BrushMode brushMode;
        public bool isConnectedToLast;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref prevX);
            serializer.SerializeValue(ref prevY);
            serializer.SerializeValue(ref newX);
            serializer.SerializeValue(ref newY);
            serializer.SerializeValue(ref brushSize);
            serializer.SerializeValue(ref brushMode);
            serializer.SerializeValue(ref isConnectedToLast);
        }
    }

    [Serializable]
    private enum BrushMode
    {
        Draw,
        Erase
    }

    [SerializeField] private TextureSettings textureSettings = new TextureSettings
    {
        sizeX = 512,
        sizeY = 1024,
        wrapMode = TextureWrapMode.Clamp,
        filterMode = FilterMode.Point
    };
    [SerializeField] private Texture2D texture;
    //private Texture2D materialTextureCopy;
    //private Texture materialMainTexture;

    [SerializeField] private Color drawColor;
    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Material material;

    [SerializeField] private Camera _camera;
    [SerializeField] private Collider _collider;
    [SerializeField] private BrushMode brushMode = BrushMode.Draw;
    [SerializeField] private byte brushSize = 12;
    private int halfBrushSize;
    private bool isConnectedToLast;
    private bool isPainter;

    private NetworkVariable<Vector2Short> rayPos = new NetworkVariable<Vector2Short>(new Vector2Short(), NetworkVariableReadPermission.Everyone);
    private DrawingParams drawingParams;

    private bool isDraw = false;

    [SerializeField] private bool drawOnTransparent;

    [Header("Set if functionality is needed")]
    //[SerializeField] private Slider brushSizeSlider;
    [SerializeField] private Button switchBrushModeButton;
    [SerializeField] private Button saveAsPNGButton;
    [SerializeField] private Button clearCanvasButton;
    [HideInInspector] public UnityEvent OnNetworkSpawned;

    [SerializeField] private Sprite chalkSprite;
    [SerializeField] private Sprite eraserSprite;
    private Image switchBrushModeButtonImage;

    public byte BrushSize => brushSize;

    private void Awake()
    {
        drawColor.a = 1f;
        OnValidate();

        material.color = Color.white;

        InitControlUI();
    }

    //public override void OnDestroy()
    //{
    //    if (materialMainTexture != null)
    //        material.mainTexture = materialMainTexture;
    //}

    private void OnValidate()
    {
        if (drawOnTransparent)
        {
            baseColor.a = 0f;
        }
        else
        {
            baseColor.a = 1f;
        }
    }

    private void InitControlUI()
    {
        clearCanvasButton?.onClick.AddListener(ClearCanvas);
        saveAsPNGButton?.onClick.AddListener(SavePaintingAsPng);

        switchBrushModeButton?.onClick.AddListener(SwitchBrushMode);
        switchBrushModeButtonImage = switchBrushModeButton?.GetComponent<Image>();
    }

    public override void OnNetworkSpawn()
    {
        Logger.Instance.Log($"[{nameof(Paint)}] Spawned on {(IsServer ? "Server" : "Client")}");
        Scene loadedScene = SceneManager.GetActiveScene();

        if (IsServer)
        {
            isPainter = true;
            rayPos.OnValueChanged += OnMousePositionValueChanged;
        }

        CreateTexture();
        halfBrushSize = brushSize / 2;

        if (loadedScene.name != RelayManager.Instance.LobbySceneName)
            this.enabled = false;
        else
            isPainter = true;

        OnNetworkSpawned?.Invoke();
    }

    private void OnMousePositionValueChanged(Vector2Short previousValue, Vector2Short newValue)
    {
        drawingParams.prevX = previousValue.x;
        drawingParams.prevY = previousValue.y;
        drawingParams.newX = newValue.x;
        drawingParams.newY = newValue.y;
        drawingParams.brushSize = (byte)brushSize;
        drawingParams.brushMode = brushMode;
        drawingParams.isConnectedToLast = isConnectedToLast;

        DrawCircleClientRpc(drawingParams);

        isConnectedToLast = true;
    }

    private void CreateTexture()
    {
        if (!material)
        {
            Logger.Instance.LogError($"[{nameof(Paint)}] Material isn't set");
            return;
        }

        texture = new Texture2D(textureSettings.sizeX, textureSettings.sizeY);
        texture.wrapMode = textureSettings.wrapMode;
        texture.filterMode = textureSettings.filterMode;

        material.mainTexture = texture;
        Fill(baseColor);

        //if (material.mainTexture == null)
        //{

        //}
        //else
        //{
        //    //CreateTexturesFromMaterialTexture();

        //    //materialMainTexture = material.mainTexture;
        //    //material.mainTexture = texture;
        //}
    }

    //private void CreateTexturesFromMaterialTexture()
    //{
    //    Color32[] materialTextureColors = (material.mainTexture as Texture2D).GetPixels32();

    //    Logger.Instance.Log(material.mainTexture.width + " by " + material.mainTexture.height);
    //    texture = new Texture2D(material.mainTexture.height, material.mainTexture.width);

    //    texture.filterMode = material.mainTexture.filterMode;
    //    texture.wrapMode = material.mainTexture.wrapMode;
    //    texture.anisoLevel = material.mainTexture.anisoLevel;

    //    texture.SetPixels32(materialTextureColors);
    //    texture.Apply();

    //    materialTextureCopy = new Texture2D(material.mainTexture.height, material.mainTexture.width);

    //    materialTextureCopy.filterMode = material.mainTexture.filterMode;
    //    materialTextureCopy.wrapMode = material.mainTexture.wrapMode;
    //    materialTextureCopy.anisoLevel = material.mainTexture.anisoLevel;

    //    materialTextureCopy.SetPixels32(materialTextureColors);
    //}

    private void Update()
    {
        if (!isPainter)
            return;

        Draw();
    }

    private void Draw()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            isDraw = true;
            //Logger.Instance.Log($"[{nameof(Paint)}] Start Curve");
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            SendIsConnectedToLastServerRpc(false);
            isDraw = false;
            //Logger.Instance.Log($"[{nameof(Paint)}] End Curve");
        }

        if (isDraw)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (_collider.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                int rayX = (int)(hitInfo.textureCoord.x * textureSettings.sizeX);
                int rayY = (int)(hitInfo.textureCoord.y * textureSettings.sizeY);

                Vector2Short newRayPos = new Vector2Short(rayX, rayY);

                if (Vector2Short.Distance(rayPos.Value, newRayPos) > halfBrushSize)
                {
                    if (IsServer)
                        rayPos.Value = newRayPos;
                    else
                        SendRayPosServerRpc(newRayPos.x, newRayPos.y);

                    //Logger.Instance.Log($"[{nameof(Paint)}] Continue Curve on pixel {newRayPos}");
                }
            }
            //else
            //{
            //    Logger.Instance.Log($"[{nameof(Paint)}] {hitInfo.collider.gameObject.name}");
            //}
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendRayPosServerRpc(short x, short y)
    {
        rayPos.Value = new Vector2Short(x, y);
    }

    [ServerRpc (RequireOwnership = false)]
    private void SendIsConnectedToLastServerRpc(bool value)
    {
        isConnectedToLast = value;
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
    
    private void DrawCircle(short rayX, short rayY)
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
                    texture.SetPixel(rayX + x - halfBrushSize, rayY + y - halfBrushSize, color);
                }
            }
        }
    }

    [ClientRpc]
    private void DrawCircleClientRpc(DrawingParams drawingParams)
    {
        if (drawingParams.isConnectedToLast)
            SmoothDrawCircle(drawingParams.prevX, drawingParams.prevY, drawingParams.newX, drawingParams.newY);
        else
            DrawCircle(drawingParams.newX, drawingParams.newY);

        //Debug.Log($"{drawingParams.prevX}, {drawingParams.prevY}, {drawingParams.newX}, {drawingParams.newY}, {drawingParams.isConnectedToLast}");

        texture.Apply();
    }

    private void SmoothDrawCircle(short pRayX, short pRayY, short rayX, short rayY)
    {
        Vector2Short prevPoint = new Vector2Short(pRayX, pRayY);
        Vector2Short newPoint = new Vector2Short(rayX, rayY);
        Vector2Short currentPoint = new Vector2Short();
        float step = 1f / Vector2Short.Distance(prevPoint, newPoint) / halfBrushSize;

        for (float t = 0; t <= 1f; t += step)
        {
            currentPoint.x = (short)Mathf.Lerp(prevPoint.x, newPoint.x, t);
            currentPoint.y = (short)Mathf.Lerp(prevPoint.y, newPoint.y, t);

            DrawCircle(currentPoint.x, currentPoint.y);
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
        
        Logger.Instance.Log($"[{nameof(Paint)}] Path: " + dirPath);
    }

    public void SwitchBrushMode()
    {
        if (brushMode == BrushMode.Draw)
            SwitchBrushServerRpc(BrushMode.Erase);
        else
            SwitchBrushServerRpc(BrushMode.Draw);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwitchBrushServerRpc(BrushMode brushMode)
    {
        SwitchBrushClientRpc(brushMode);
    }

    [ClientRpc]
    private void SwitchBrushClientRpc(BrushMode brushMode)
    {
        this.brushMode = brushMode;

        if (brushMode == BrushMode.Draw)
            switchBrushModeButtonImage.sprite = eraserSprite;
        else
            switchBrushModeButtonImage.sprite = chalkSprite;
    }

    public void ChangeSize(float brushSize)
    {
        ChangeSizeServerRpc((byte)brushSize);
    }

    public void ChangeSize(byte brushSize)
    {
        ChangeSizeServerRpc(brushSize);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeSizeServerRpc(byte brushSize)
    {
        ChangeSizeClientRpc(brushSize);
    }

    [ClientRpc]
    private void ChangeSizeClientRpc(byte brushSize)
    {
        this.brushSize = brushSize;
        halfBrushSize = brushSize / 2;

        //Debug.Log("Size Changed To: " + brushSize);
    }

    public void ClearCanvas()
    {
        if (IsServer)
        {
            ClearCanvasClientRpc();
            isConnectedToLast = false;
        }
        else
        {
            ClearCanvasServerRpc();
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void ClearCanvasServerRpc()
    {
        ClearCanvasClientRpc();
        isConnectedToLast = false;
    }

    [ClientRpc]
    private void ClearCanvasClientRpc()
    {
        Fill(baseColor);
    }

    public void SetActive(bool isPainter)
    {
        this.isPainter = isPainter;
    }

    public Texture2D GetTexture()
    {
        return texture;
    }
}