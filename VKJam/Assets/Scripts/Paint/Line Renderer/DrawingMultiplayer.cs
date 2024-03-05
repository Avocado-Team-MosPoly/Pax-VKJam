using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class DrawingMultiplayer : NetworkBehaviour
{
    #region Netcode Fields

    public UnityEvent OnNetworkSpawned { get; private set; }
    private NetworkVariable<Vector3> painterMousePosition = new(new Vector3(), NetworkVariableReadPermission.Everyone);

    #endregion

    // with Netcode use "Set Brush Size" method
    public float BrushSize
    {
        get
        {
            return brushSize;
        }
        set
        {
            brushSize = value;
            brushLine.startWidth = brushLine.endWidth = brushSize * initialBrushSize;
        }
    }
    private float brushSize = 1f;

    [Header("Buttons")]

    [SerializeField] private Button clearCanvasButton;
    [SerializeField] private Button switchBrushModeButton;
    private Image switchBrushModeButtonImage;
    
    [SerializeField] private Sprite chalkSprite;
    [SerializeField] private Sprite eraserSprite;

    [Header("Cameras")]

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
    private BrushMode brushMode = BrushMode.Draw;

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
        BrushSize = 1f;

        brushMaterial.color = brushColor;

        canvasFill.SetActive(false);
        canvasFillMaterial.color = backgroundColor;

        canvasMaterial.mainTexture = renderTexture;

        switchBrushModeButtonImage = switchBrushModeButton.GetComponent<Image>();

        clearCanvasButton.onClick.AddListener(ClearCanvasGlobal);
        switchBrushModeButton.onClick.AddListener(SwitchBrushMode);
    }

    private void Awake() => Init();

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
    protected virtual void StartDrawingServerRpc(Vector3 startPos, ServerRpcParams rpcParams) => StartDrawingClientRpc(startPos);

    [ServerRpc(RequireOwnership = false)]
    protected virtual void StopDrawingServerRpc() => StopDrawingClientRpc();

    [ClientRpc]
    private void StartDrawingClientRpc(Vector3 startPos)
    {
        if (IsPainter)
            return;

        lastPosition = startPos;
        MoveBrush(lastPosition);
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

        lastPosition = hitInfo.point - drawingCamera.transform.position;
        MoveBrush(lastPosition);
        brushObject.SetActive(true);
        isDrawing = true;

        // if (IsServer)
        //     StartDrawingClientRpc(hitInfo.point - drawingCamera.transform.position);
        // else
        StartDrawingServerRpc(lastPosition, new ServerRpcParams());
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
                Logger.Instance.Log(this, lastPosition);
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
        Vector3 offset = drawingCamera.transform.position + drawingCamera.transform.forward;

        brushLine.SetPositions(new Vector3[] { lastPosition + offset, targetPosition + offset });
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

    [ClientRpc]
    private void SetBrushColorClientRpc(Color32 color)
    {
        brushColor = color;
        brushMaterial.color = brushColor;
        //brushMaterial.SetColor("_Color", brushColor);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBrushColorServerRpc(Color32 color) => SetBrushColorClientRpc(color);

    public void SetBrushColor(Color32 color)
    {
        if (IsServer)
            SetBrushColorClientRpc(color);
        else
            SetBrushColorServerRpc(color);
    }

    [ClientRpc]
    private void SetBackgroundColorClientRpc(Color32 color)
    {
        backgroundColor = color;
        canvasFillMaterial.color = backgroundColor;
        // canvasMaterial.SetColor("_BackgroundColor", backgroundColor);
        // canvasFillMaterial.SetColor("_Color", backgroundColor);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBackgroundColorServerRpc(Color32 color) => SetBrushColorClientRpc(color);

    public void SetBackgroundColor(Color32 color)
    {
        if (IsServer)
            SetBackgroundColorClientRpc(color);
        else
            SetBackgroundColorServerRpc(color);
    }

    #endregion

    #region Set Brush Size

    [ClientRpc]
    private void SetBrushSizeClientRpc(float value) => BrushSize = value;

    [ServerRpc(RequireOwnership = false)]
    private void SetBrushSizeServerRpc(float value) => SetBrushSizeClientRpc(value);

    public void SetBrushSize(float value)
    {
        if (IsServer)
            SetBrushSizeClientRpc(value);
        else
            SetBrushSizeServerRpc(value);
    }

    #endregion

    #region Switch Brush Mode

    [ClientRpc]
    private void SwitchBrushClientRpc(BrushMode brushMode)
    {
        this.brushMode = brushMode;

        if (brushMode == BrushMode.Draw)
        {
            brushLine.sharedMaterial = brushMaterial;
            switchBrushModeButtonImage.sprite = eraserSprite;
        }
        else
        {
            brushLine.sharedMaterial = canvasFillMaterial;
            switchBrushModeButtonImage.sprite = chalkSprite;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwitchBrushServerRpc(BrushMode brushMode) => SwitchBrushClientRpc(brushMode);

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