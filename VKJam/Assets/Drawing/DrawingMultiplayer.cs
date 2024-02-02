using System.Collections;
using System.Collections.Generic;
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
            return brush.transform.localScale.z;
        }
        set
        {
            brush.transform.localScale = value * Vector3.one;
        }
    }

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera drawingCamera;
    //[SerializeField] private FPSManager fpsManager;

    [Header("Canvas Settings")]
    [SerializeField] private Collider drawingCollider;
    [SerializeField] private Material canvasMaterial;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Color backgroundColor = Color.white;

    [Header("Brush Settings")]
    [SerializeField] private GameObject brush;
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

    public override void OnNetworkSpawn()
    {
        painterMousePosition.OnValueChanged += OnMousePositionValueChanged;

        isEnabled = IsServer;
        ClearCanvas();
        OnNetworkSpawned?.Invoke();
    }

    private void OnMousePositionValueChanged(Vector3 previousValue, Vector3 newValue)
    {
        if (GameManager.Instance.IsPainter)
            return;

        if (!isDrawing)
        {
            brush.transform.localPosition = newValue;
        }

        lastPosition = newValue;
        isDrawing = true;
        brush.SetActive(true);
    }

    private void Init()
    {
        brush.SetActive(false);
        brushRenderer = brush.GetComponent<MeshRenderer>();
        BrushSize = 0.5f;
        lastPosition = brush.transform.localPosition;
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
        StartCoroutine(ClearCanvasCoroutine());
    }

    private void Update()
    {
        if (isDrawing)
            Draw();

        if (!isEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
            StartDrawing();
        else if (isDrawing && Input.GetKeyUp(KeyCode.Mouse0))
            StopDrawing();

        // Draw();
    }

    private void StartDrawing()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!drawingCollider.Raycast(ray, out RaycastHit hitInfo, 100f))
            return;

        brush.transform.position = hitInfo.point + drawingCamera.transform.forward;
        brush.SetActive(true);

        isDrawing = true;
    }

    private void StopDrawing()
    {
        brush.SetActive(false);

        isDrawing = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPainterMousePositionServerRpc(Vector3 position)
    {
        painterMousePosition.Value = position;
    }

    private void Draw()
    {
        if (!isDrawing)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (drawingCollider.Raycast(ray, out RaycastHit hitInfo, mainCamera.farClipPlane))
        {
            Vector3 newPosition = hitInfo.point + drawingCamera.transform.forward - drawingCamera.transform.position;

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
        targetPosition += drawingCamera.transform.position;
        float distance = Vector3.Distance(brush.transform.position, targetPosition);
        if (distance < stopDistance)
        {
            if (!GameManager.Instance.IsPainter)
                StopDrawing();

            return;
        }
        float brushTranslation = BrushSize / brushSizeCoefficient;

        Vector3 direction = (targetPosition - brush.transform.position).normalized;
        Vector3 newPosition = brush.transform.position + brushTranslation * direction;
        
        float distance1 = Vector3.Distance(newPosition, targetPosition);

        if (brushTranslation > distance1)
            newPosition = targetPosition;

        brush.transform.position = newPosition;

        // brush.transform.position = Vector3.Lerp(brush.transform.position, targetPosition,
        //                                         (Time.deltaTime * lerpRate) *
        //                                         (fpsManager.CurrentFPS / idealFPS) *
        //                                         (distanceCoefficient / distance) *
        //                                         BrushSize);
    }

    #region Enable/Disable

    public void Enable(bool clearCanvas)
    {
        isEnabled = true;

        if (clearCanvas)
            ClearCanvas();
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
        StartCoroutine(ClearCanvasCoroutine());
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
        StartCoroutine(ClearCanvasCoroutine());
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