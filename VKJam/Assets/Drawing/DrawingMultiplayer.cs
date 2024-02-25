using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;

public struct Vector3Short : INetworkSerializable
{
    public short x;
    public short y;
    public short z;

    public Vector3Short(int x, int y, int z)
    {
        this.x = (short)x;
        this.y = (short)y;
        this.z = (short)z;
    }
    public Vector3Short(short x, short y, short z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static float Distance(Vector3Short a, Vector3Short b)
    {
        Vector3Short direction = b - a;
        float distance = Mathf.Sqrt(direction.x * direction.x + direction.y * direction.y + direction.z * direction.z);
        
        return distance;
    }

    public static Vector3Short operator - (Vector3Short a, Vector3Short b)
    {
        return new Vector3Short
        {
            x = (short)(b.x - a.x),
            y = (short)(b.y - a.y),
            z = (short)(b.z - a.z)
        };
    }

    public static explicit operator Vector3Short(Vector3 vector3)
    {
        return new Vector3Short((short)vector3.x, (short)vector3.y, (short)vector3.z);
    }
    public static implicit operator Vector3(Vector3Short vector3Short)
    {
        return new Vector3(vector3Short.x, vector3Short.y, vector3Short.z);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref x);
        serializer.SerializeValue(ref y);
        serializer.SerializeValue(ref z);
    }

    public override string ToString()
    {
        return $"({nameof(Vector3Short)}({x}, {y}, {z})";
    }
}

[Serializable]
public struct RenderTextureSettings
{
    public int width;
    public int height;
    public RenderTextureFormat format;
    public TextureWrapMode wrapMode;
    public FilterMode filterMode;
}

public class DrawingMultiplayer : NetworkBehaviour
{
    private NetworkVariable<Vector3> painterMousePosition = new(new Vector3(), NetworkVariableReadPermission.Everyone);

    [HideInInspector] public UnityEvent OnNetworkSpawned;

    [SerializeField] private BrushMode brushMode = BrushMode.Draw;

    [SerializeField] private Button switchBrushModeButton;
    private Image switchBrushModeButtonImage;
    
    [SerializeField] private Sprite chalkSprite;
    [SerializeField] private Sprite eraserSprite;

    public float BrushSize
    {
        get
        {
            return brushSize;
        }
        set
        {
            brushSize = value;
            brushLine.startWidth = brushLine.endWidth = brushSize;
        }
    }
    private float brushSize;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera drawingCamera;
    
    [SerializeField] private float raycastMaxDistance = 10f;

    [Header("Canvas Settings")]

    [Tooltip("In-game size: 2048x1308. Lobby size: 2048x2048")]
    [SerializeField] private RenderTextureSettings renderTextureSettings = new()
    {
        width = 2048,
        height = 2048,
        format = RenderTextureFormat.ARGB32,
        wrapMode = TextureWrapMode.Clamp,
        filterMode = FilterMode.Bilinear
    };

    [SerializeField] private Collider drawingCollider;
    [SerializeField] private Material canvasMaterial;
    [SerializeField] private Color backgroundColor = Color.white;

    private RenderTexture renderTexture;

    [Header("Brush Settings")]
    [SerializeField] private LineRenderer brushLine;
    [SerializeField] private float initialBrushSize = 0.05f;
    [SerializeField] private float stopDistance = 0.02f;

    private GameObject brushObject;

    [Header("Brush Material Settings")]
    [SerializeField] private Material brushMaterial;
    [SerializeField] private Color brushColor = Color.black;

    [Header("Canvas Fill")]
    [SerializeField] private GameObject canvasFill;
    [SerializeField] private Material canvasFillMaterial;

    private bool isDrawing;
    private Vector3 lastPosition;
    
    protected bool isEnabled;

    private bool IsPainter => GameManager.Instance != null && GameManager.Instance.IsPainter;
    private bool IsMouseOverUI => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

    public override void OnNetworkSpawn()
    {
        painterMousePosition.OnValueChanged += OnMousePositionValueChanged;

        isEnabled = IsServer;
        OnNetworkSpawned?.Invoke();
    }

    protected virtual void OnMousePositionValueChanged(Vector3 previousValue, Vector3 newValue)
    {
        if (IsPainter)
            return;

        MoveBrush(newValue);
    }

    private void Init()
    {
        renderTexture = new RenderTexture(renderTextureSettings.width, renderTextureSettings.height, 0, renderTextureSettings.format)
        {
            wrapMode = renderTextureSettings.wrapMode,
            filterMode = renderTextureSettings.filterMode
        };
        drawingCamera.targetTexture = renderTexture;
        canvasMaterial.mainTexture = renderTexture;

        brushObject = brushLine.gameObject;

        brushObject.SetActive(false);
        BrushSize = initialBrushSize;

        brushMaterial.SetColor("_Color", brushColor);

        canvasFill.SetActive(false);
        canvasFillMaterial.SetColor("_Color", backgroundColor);

        canvasMaterial.SetTexture("_BaseMap", renderTexture);

        switchBrushModeButton.onClick.AddListener(SwitchBrushMode);
        switchBrushModeButtonImage = switchBrushModeButton.GetComponent<Image>();

    }

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        if (!isEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
            StartDrawing();
        else if (isDrawing && Input.GetKeyUp(KeyCode.Mouse0))
            StopDrawing();

        Draw();
    }

    #region Start/Stop Drawing

    [ServerRpc(RequireOwnership = false)]
    protected virtual void StartDrawingServerRpc(Vector3 startPos, ServerRpcParams rpcParams)
    {
        StartDrawingClientRpc(startPos);
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void StopDrawingServerRpc()
    {
        StopDrawingClientRpc();
    }

    [ClientRpc]
    private void StartDrawingClientRpc(Vector3 startPos)
    {
        if (IsPainter)
            return;

        lastPosition = startPos + drawingCamera.transform.position + drawingCamera.transform.forward;
        brushLine.SetPositions(new Vector3[] { lastPosition, lastPosition });
        brushObject.SetActive(true);
        isDrawing = true;
    }

    [ClientRpc]
    private void StopDrawingClientRpc()
    {
        if (IsPainter)
            return;

        brushObject.SetActive(false);
        isDrawing = false;
    }

    private void StartDrawing()
    {
        if (isDrawing || IsMouseOverUI)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!drawingCollider.Raycast(ray, out RaycastHit hitInfo, raycastMaxDistance))
            return;

        lastPosition = hitInfo.point + drawingCamera.transform.forward;
        brushLine.SetPositions(new Vector3[] { lastPosition, lastPosition });
        brushObject.SetActive(true);
        isDrawing = true;

        // if (IsServer)
        //     StartDrawingClientRpc(hitInfo.point - drawingCamera.transform.position);
        // else
        StartDrawingServerRpc(hitInfo.point - drawingCamera.transform.position, new ServerRpcParams());
    }

    private void StopDrawing()
    {
        if (!isDrawing)
            return;

        brushObject.SetActive(false);
        isDrawing = false;

        // if (IsServer)
        //     StopDrawingClientRpc();
        // else
        StopDrawingServerRpc();
    }

    #endregion

    #region Drawing

    [ServerRpc(RequireOwnership = false)]
    private void SendPainterMousePositionServerRpc(Vector3 position) => painterMousePosition.Value = position;

    private void Draw()
    {
        if (!isDrawing)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (drawingCollider.Raycast(ray, out RaycastHit hitInfo, raycastMaxDistance))
        {
            Vector3 newPosition = hitInfo.point - drawingCamera.transform.position;

            if (Vector3.Distance(lastPosition, newPosition) > stopDistance)
            {
                MoveBrush(newPosition);

                if (IsServer)
                    painterMousePosition.Value = newPosition;
                else
                    SendPainterMousePositionServerRpc(newPosition);
            }
        }
    }

    protected void MoveBrush(Vector3 targetPosition)
    {
        targetPosition += drawingCamera.transform.position + drawingCamera.transform.forward;

        brushLine.SetPositions(new Vector3[] { lastPosition, targetPosition });
        lastPosition = targetPosition;
    }

    #endregion

    #region Enable/Disable

    public void Enable(bool clearCanvas)
    {
        isEnabled = true;

        if (clearCanvas)
            ClearCanvasGlobal();
    }

    public void Disable()
    {
        isEnabled = false;
    }

    #endregion

    #region Set Color

    public void SetBrushColor(Color color)
    {
        brushColor = color;
        brushMaterial.SetColor("_Color", brushColor);
    }

    public void SetBackgroundColor(Color color)
    {
        backgroundColor = color;
        canvasMaterial.SetColor("_BackgroundColor", backgroundColor);
        canvasFillMaterial.SetColor("_Color", backgroundColor);
    }

    #endregion

    #region Switch Brush Mode

    [ClientRpc]
    private void SwitchBrushClientRpc(BrushMode brushMode)
    {
        this.brushMode = brushMode;

        if (brushMode == BrushMode.Draw)
        {
            // brushRenderer.sharedMaterial = brushMaterial;
            switchBrushModeButtonImage.sprite = eraserSprite;
        }
        else
        {
            // brushRenderer.sharedMaterial = canvasFillMaterial;
            switchBrushModeButtonImage.sprite = chalkSprite;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwitchBrushServerRpc(BrushMode brushMode)
    {
        SwitchBrushClientRpc(brushMode);
    }

    public void SwitchBrushMode()
    {
        if (brushMode == BrushMode.Draw)
            SwitchBrushServerRpc(BrushMode.Erase);
        else
            SwitchBrushServerRpc(BrushMode.Draw);
    }

    #endregion

    #region Clear Canvas

    [ClientRpc]
    private void ClearCanvasClientRpc(byte senderClientId)
    {
        if (IsServer || senderClientId == NetworkManager.LocalClientId)
            return;

        ClearCanvasLocal();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClearCanvasServerRpc(ServerRpcParams rpcParams)
    {
        ClearCanvasLocal();
        ClearCanvasClientRpc((byte)rpcParams.Receive.SenderClientId);
    }

    [ContextMenu("Clear Canvas Global")]
    public void ClearCanvasGlobal()
    {
        ClearCanvasLocal();

        if (IsServer)
            ClearCanvasClientRpc((byte)NetworkManager.LocalClientId);
        else
            ClearCanvasServerRpc(new ServerRpcParams());
    }

    [ContextMenu("Clear Canvas Local")]
    public void ClearCanvasLocal()
    {
        Debug.Log("Clearing Canvas...");
        canvasFill.SetActive(true);
        drawingCamera.Render();
        canvasFill.SetActive(false);
    }

    #endregion

    #region Fill Canvas

    [ClientRpc]
    private void FillCanvasClientRpc(Color color, byte senderClientId)
    {
        if (IsServer || senderClientId == NetworkManager.LocalClientId)
            return;

        FillCanvasLocal(color);
    }

    [ServerRpc(RequireOwnership = false)]
    private void FillCanvasServerRpc(Color color, ServerRpcParams rpcParams)
    {
        FillCanvasLocal(color);
        FillCanvasClientRpc(color, (byte)rpcParams.Receive.SenderClientId);
    }

    public void FillCanvasGlobal(Color color)
    {
        if (IsServer)
            FillCanvasClientRpc(color, (byte)NetworkManager.LocalClientId);
        else
            FillCanvasServerRpc(color, new ServerRpcParams());
    }

    public void FillCanvasLocal(Color color)
    {
        SetBackgroundColor(color);
        ClearCanvasLocal();
    }

    #endregion
}