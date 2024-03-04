using Unity.Netcode;
using UnityEngine;

public class DrawingTutorial : MonoBehaviour
{
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

    [Header("Canvas Settings")]
    [Tooltip("Default size: 2048x1308")]
    [SerializeField] private RenderTextureSettings renderTextureSettings = new()
    {
        width = 2048,
        height = 1308,
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

    private bool isEnabled;

    private void Awake()
    {
        Init();
        ClearCanvas();
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

    private void Init()
    {
        renderTexture = new RenderTexture(renderTextureSettings.width, renderTextureSettings.height, 0, renderTextureSettings.format)
        {
            wrapMode = renderTextureSettings.wrapMode,
            filterMode = renderTextureSettings.filterMode
        };

        drawingCamera.targetTexture = renderTexture;

        brushObject = brushLine.gameObject;
        brushObject.SetActive(false);

        BrushSize = initialBrushSize;
        brushMaterial.color = brushColor;

        canvasFill.SetActive(false);
        canvasFillMaterial.color = backgroundColor;

        canvasMaterial.mainTexture = renderTexture;
    }

    private void StartDrawing()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!drawingCollider.Raycast(ray, out RaycastHit hitInfo, mainCamera.farClipPlane))
            return;

        lastPosition = hitInfo.point + drawingCamera.transform.forward;

        brushLine.SetPositions(new Vector3[] { lastPosition, lastPosition });
        brushObject.SetActive(true);

        isDrawing = true;
    }

    private void StopDrawing()
    {
        brushObject.SetActive(false);

        isDrawing = false;
    }

    private void Draw()
    {
        if (!isDrawing)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (drawingCollider.Raycast(ray, out RaycastHit hitInfo, mainCamera.farClipPlane))
            MoveBrush(hitInfo.point + drawingCamera.transform.forward);
    }

    private void MoveBrush(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(lastPosition, targetPosition);
        if (distance < stopDistance)
            return;

        brushLine.SetPositions(new Vector3[] { lastPosition, targetPosition });
        lastPosition = targetPosition;
    }

    [ContextMenu("Enable")]
    public void Enable(/*bool clearCanvas*/)
    {
        isEnabled = true;

        // if (clearCanvas)
            ClearCanvas();
    }

    [ContextMenu("Disable")]
    public void Disable()
    {
        isEnabled = false;
    }

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

    [ContextMenu("Clear Canvas")]
    public void ClearCanvas()
    {
        canvasFill.SetActive(true);
        drawingCamera.Render();
        canvasFill.SetActive(false);
    }

    public void FillCanvas(Color color)
    {
        SetBackgroundColor(color);
        ClearCanvas();
    }
}