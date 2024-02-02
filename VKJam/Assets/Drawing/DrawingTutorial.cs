using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingTutorial : MonoBehaviour
{
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
    [SerializeField] private float stopDistance = 0.02f;
    //[SerializeField, Range(0.1f, 10f)] private float lerpRate = 5f;

    private List<GameObject> brushClones = new();

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

    private void Init()
    {
        brush.SetActive(false);
        BrushSize = 0.5f;
        lastPosition = brush.transform.position;
        brushMaterial.SetColor("_Color", brushColor);

        canvasFill.SetActive(false);
        canvasFillMaterial.SetColor("_Color", backgroundColor);

        canvasMaterial.SetTexture("_BaseMap", renderTexture);
    }

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

    private void Draw()
    {
        if (!isDrawing)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (drawingCollider.Raycast(ray, out RaycastHit hitInfo, mainCamera.farClipPlane))
        {
            lastPosition = hitInfo.point + drawingCamera.transform.forward;
        }

        MoveBrush(lastPosition);
    }

    private void MoveBrush(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(brush.transform.position, targetPosition);
        if (distance < stopDistance)
            return;

        float brushTranslation = BrushSize / brushSizeCoefficient;

        Vector3 direction = (targetPosition - brush.transform.position).normalized;
        Vector3 newPosition = brush.transform.position + brushTranslation * direction;
        
        float distance1 = (newPosition - targetPosition).magnitude;

        if (brushTranslation > distance1)
            newPosition = targetPosition;

        brush.transform.position = newPosition;

        // brush.transform.position = Vector3.Lerp(brush.transform.position, targetPosition,
        //                                         (Time.deltaTime * lerpRate) *
        //                                         (fpsManager.CurrentFPS / idealFPS) *
        //                                         (distanceCoefficient / distance) *
        //                                         BrushSize);
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

    private IEnumerator ClearCanvasCoroutine()
    {
        canvasFill.SetActive(true);

        yield return new WaitForEndOfFrame();

        canvasFill.SetActive(false);
    }

    [ContextMenu("Clear Canvas")]
    public void ClearCanvas()
    {
        StartCoroutine(ClearCanvasCoroutine());
    }

    public void FillCanvas(Color color)
    {
        SetBackgroundColor(color);
        ClearCanvas();
    }
}