using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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

public class DrawingMultiplayer : NetworkBehaviour
{
    #region Multiplayer Fields

    private NetworkVariable<Vector3> painterMousePosition = new(new Vector3(), NetworkVariableReadPermission.Everyone);

    [HideInInspector] public UnityEvent OnNetworkSpawned;

    #endregion

    [SerializeField] private BrushMode brushMode = BrushMode.Draw;

    [SerializeField] private Button switchBrushModeButton;
    private Image switchBrushModeButtonImage;
    
    [SerializeField] private Sprite chalkSprite;
    [SerializeField] private Sprite eraserSprite;

    public float BrushSize
    {
        get
        {
            return brushTransform.localScale.z;
        }
        set
        {
            brushTransform.localScale = value * Vector3.one;
        }
    }

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera drawingCamera;
    //[SerializeField] private FPSManager fpsManager;
    [SerializeField] private float raycastMaxDistance = 10f;

    [Header("Canvas Settings")]
    [SerializeField] private Collider drawingCollider;
    [SerializeField] private Material canvasMaterial;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Color backgroundColor = Color.white;

    [Header("Brush Settings")]
    [SerializeField] private GameObject brush;
    private Transform brushTransform;
    private MeshRenderer brushRenderer;
    [SerializeField] private float stopDistance = 0.02f;
    //[SerializeField, Range(0.1f, 10f)] private float lerpRate = 5f;

    [Header("Brush Material Settings")]
    [SerializeField] private Material brushMaterial;
    [SerializeField] private Color brushColor = Color.black;

    [Header("Canvas Fill")]
    [SerializeField] private GameObject canvasFill;
    [SerializeField] private Material canvasFillMaterial;

    private bool isDrawing;
    private Vector3 lastPosition;
    // private Vector3 offset = Vector3.forward;

    // private const float idealFPS = 60f;
    // private const float distanceCoefficient = 0.33f;
    private const float brushSizeCoefficient = 50f;

    private bool isEnabled;

    private bool IsPainter => GameManager.Instance.IsPainter;

    public override void OnNetworkSpawn()
    {
        painterMousePosition.OnValueChanged += OnMousePositionValueChanged;

        isEnabled = IsServer;
        ClearCanvas();
        OnNetworkSpawned?.Invoke();
    }

    private void OnMousePositionValueChanged(Vector3 previousValue, Vector3 newValue)
    {
        if (IsPainter)
            return;

        lastPosition = newValue;
        //MoveBrush(newValue);
    }

    private void Init()
    {
        brushTransform = brush.transform;

        brush.SetActive(false);
        brushRenderer = brush.GetComponent<MeshRenderer>();
        BrushSize = 0.5f;
        lastPosition = brushTransform.localPosition;
        brushMaterial.SetColor("_Color", brushColor);

        canvasFill.SetActive(false);
        canvasFillMaterial.SetColor("_Color", backgroundColor);

        canvasMaterial.SetTexture("_BaseMap", renderTexture);

        // new

        switchBrushModeButton.onClick.AddListener(SwitchBrushMode);
        switchBrushModeButtonImage = switchBrushModeButton.GetComponent<Image>();
    }

    private void Awake()
    {
        Init();
        GameManager.Instance.StartCoroutine(ClearCanvasCoroutine());
    }

    private void Update()
    {
        if (!isEnabled)
        {
            if (isDrawing)
            {
                MoveBrush(lastPosition);
            }

            return;
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
            StartDrawing();
        else if (Input.GetKeyUp(KeyCode.Mouse0))
            StopDrawing();

        Draw();
    }

    #region Start/Stop Drawing

    [ClientRpc] private void StartDrawingClientRpc(Vector3 startPos)
    {
        if (IsPainter)
            return;

        lastPosition = startPos + drawingCamera.transform.position + drawingCamera.transform.forward;
        brushTransform.position = lastPosition;
        brush.SetActive(true);
        isDrawing = true;
    }

    [ClientRpc] private void StopDrawingClientRpc()
    {
        if (IsPainter)
            return;

        brush.SetActive(false);
        isDrawing = false;
    }

    [ServerRpc(RequireOwnership = false)] private void StartDrawingServerRpc(Vector3 startPos) => StartDrawingClientRpc(startPos);
    [ServerRpc(RequireOwnership = false)] private void StopDrawingServerRpc() => StopDrawingClientRpc();

    private void StartDrawing()
    {
        if (isDrawing)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!drawingCollider.Raycast(ray, out RaycastHit hitInfo, raycastMaxDistance))
            return;

        lastPosition = hitInfo.point + drawingCamera.transform.forward;
        brushTransform.position = lastPosition;

        brush.SetActive(true);
        isDrawing = true;

        if (IsServer)
            StartDrawingClientRpc(hitInfo.point - drawingCamera.transform.position);
        else
            StartDrawingServerRpc(hitInfo.point - drawingCamera.transform.position);
    }

    private void StopDrawing()
    {
        if (!isDrawing)
            return;

        brush.SetActive(false);
        isDrawing = false;

        if (IsServer)
            StopDrawingClientRpc();
        else
            StopDrawingServerRpc();
    }

    #endregion

    #region Send Painter Mouse Position

    [ServerRpc(RequireOwnership = false)]
    private void SendPainterMousePositionServerRpc(Vector3 position)
    {
        painterMousePosition.Value = position;
    }

    #endregion

    private void Draw()
    {
        if (!isDrawing)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (drawingCollider.Raycast(ray, out RaycastHit hitInfo, raycastMaxDistance))
        {
            Vector3 newPosition = hitInfo.point - drawingCamera.transform.position; // + drawingCamera.transform.forward

            if (!(Vector3.Distance(lastPosition, newPosition) <= stopDistance))
            {
                lastPosition = newPosition;

                if (IsServer)
                    painterMousePosition.Value = lastPosition;
                else
                    SendPainterMousePositionServerRpc(lastPosition);
            }
        }

        MoveBrush(lastPosition);
    }

    private void MoveBrush(Vector3 targetPosition)
    {
        //Debug.Log(targetPosition);
        targetPosition += drawingCamera.transform.position + drawingCamera.transform.forward;
        float distance = Vector3.Distance(brushTransform.position, targetPosition);
        if (distance < stopDistance)
            return;

        float brushTranslation = BrushSize / brushSizeCoefficient;

        Vector3 direction = (targetPosition - brushTransform.position).normalized;
        Vector3 newPosition = brushTransform.position + brushTranslation * direction;
        
        float distance1 = Vector3.Distance(newPosition, targetPosition);

        if (brushTranslation > distance1)
            newPosition = targetPosition;

        brushTransform.position = newPosition;

        // brushTransform.position = Vector3.Lerp(brushTransform.position, targetPosition,
        //                                         (Time.deltaTime * lerpRate) *
        //                                         (fpsManager.CurrentFPS / idealFPS) *
        //                                         (distanceCoefficient / distance) *
        //                                         BrushSize);
    }

    #region Enable/Disable

    public void Enable(bool clearCanvas)
    {
        isEnabled = true;

        Debug.LogError("Painter");

        if (clearCanvas)
            ClearCanvas();
    }

    public void Disable()
    {
        isEnabled = false;

        Debug.LogError("Guesser");
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
            brushRenderer.sharedMaterial = brushMaterial;
            switchBrushModeButtonImage.sprite = eraserSprite;
        }
        else
        {
            brushRenderer.sharedMaterial = canvasFillMaterial;
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

    private IEnumerator ClearCanvasCoroutine()
    {
        canvasFill.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        canvasFill.SetActive(false);
    }

    [ClientRpc]
    private void ClearCanvasClientRpc()
    {
        GameManager.Instance.StartCoroutine(ClearCanvasCoroutine());
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClearCanvasServerRpc()
    {
        ClearCanvasClientRpc();
    }

    [ContextMenu("Clear Canvas")]
    public void ClearCanvas()
    {
        if (IsServer)
            ClearCanvasClientRpc();
        else
            ClearCanvasServerRpc();
    }

    #endregion

    #region Fill Canvas

    [ClientRpc]
    private void FillCanvasClientRpc(Color color)
    {
        SetBackgroundColor(color);
        GameManager.Instance.StartCoroutine(ClearCanvasCoroutine());
    }

    [ServerRpc(RequireOwnership = false)]
    private void FillCanvasServerRpc(Color color)
    {
        ClearCanvasClientRpc();
    }

    public void FillCanvas(Color color)
    {
        if (IsServer)
            FillCanvasClientRpc(color);
        else
            FillCanvasServerRpc(color);
    }

    #endregion
}