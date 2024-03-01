using System.Collections;
using UnityEngine;

public class DrawingWithMeshModification : MonoBehaviour
{
    public float BrushSize
    {
        get
        {
            return brush.transform.localScale.z;
        }
        set
        {
            // brush.transform.localScale = value * Vector3.one;
        }
    }

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera drawingCamera;

    [Header("Canvas Settings")]
    [SerializeField] private Collider drawingCollider;
    [SerializeField] private Material canvasMaterial;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Color backgroundColor = Color.white;

    [Header("Brush Settings")]
    private GameObject brush;
    [SerializeField] private float stopDistance = 0.01f;

    [Header("Brush Material Settings")]
    [SerializeField] private Material brushMaterial;
    [SerializeField] private Color brushColor = Color.black;

    [Header("Canvas Fill")]
    [SerializeField] private GameObject canvasFill;
    [SerializeField] private Material canvasFillMaterial;

    private bool isDrawing;
    private Vector3 lastPosition;

    private const float brushSizeCoefficient = 50f;

    private bool isEnabled;

    private void OnValidate()
    {
        brushMaterial.SetColor("_Color", brushColor);
        canvasFillMaterial.SetColor("_Color", backgroundColor);
    }

    private void Init()
    {
        brush = CreateBrush();
        brush.SetActive(false);
        brush.transform.parent = canvasFill.transform.parent;

        BrushSize = 1f;
        lastPosition = brush.transform.position;
        brushMaterial.SetColor("_Color", brushColor);
        //brush.transform.localScale = new Vector3(-1f, 1f, 1f);
        canvasFill.SetActive(false);
        canvasFillMaterial.SetColor("_Color", backgroundColor);

        canvasMaterial.SetTexture("_BaseMap", renderTexture);

        Enable(true);
    }

    private GameObject CreateBrush()
    {
        GameObject brush = new("Circle Brush");

        MeshFilter meshFilter = brush.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateMesh(1f, 0.075f);

        MeshRenderer meshRenderer = brush.AddComponent<MeshRenderer>();
        meshRenderer.material = brushMaterial;

        brush.layer = 6;

        return brush;
    }

    private void RegenerateVertices(Mesh mesh, float length, float width)
    {
        float radius = width / 2;
        int numVertices = 14;
        Vector3[] vertices = new Vector3[numVertices];

        float startAngle = Mathf.PI / 2;
        float angleIncrement = startAngle / 3;

        // left side
        for (int i = 0; i < numVertices / 2; i++)
        {
            float step = i * angleIncrement;
            float x = Mathf.Cos(startAngle + step);
            float y = Mathf.Sin(startAngle + step);

            vertices[i] = new Vector3(radius * x, radius * y, 0f);
        }

        // right side
        for (int i = numVertices / 2; i < numVertices; i++)
        {
            float step = (i - numVertices + 1) * angleIncrement;
            float x = Mathf.Cos(startAngle + step);
            float y = Mathf.Sin(startAngle + step);

            vertices[i] = new Vector3(length + (radius * x), radius * y, 0f);
        }

        mesh.vertices = vertices;
    }

    private Mesh CreateMesh(float length, float width)
    {
        float radius = width / 2;
        int numVertices = 14;
        Vector3[] vertices = new Vector3[numVertices];

        float startAngle = Mathf.PI / 2;
        float angleIncrement = startAngle / 3;

        // left side
        for (int i = 0; i < numVertices / 2; i++)
        {
            float step = i * angleIncrement;
            float x = Mathf.Cos(startAngle + step);
            float y = Mathf.Sin(startAngle + step);

            vertices[i] = new Vector3(radius * x, radius * y, 0f);
            // uv[i] = new Vector2(x, y);
        }

        // right side
        for (int i = numVertices / 2; i < numVertices; i++)
        {
            float step = (i - numVertices + 1) * angleIncrement;
            float x = Mathf.Cos(startAngle + step);
            float y = Mathf.Sin(startAngle + step);

            vertices[i] = new Vector3(length + (radius * x), radius * y, 0f);
            // uv[i] = new Vector2(x, y);
        }

        int[] triangles = new int[]
        {
            0, 6, 1,
            1, 6, 2,
            2, 6, 3,
            3, 6, 4,
            4, 6, 5,
            0, 7, 6,
            7, 0, 13,
            8, 7, 13,
            9, 8, 13,
            10, 9, 13,
            11, 10, 13,
            12, 11, 13
        };

        return new Mesh()
        {
            vertices = vertices,
            triangles = triangles
        };;
    }

    private void Awake() => Init();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
            Enable(true);
        else if (Input.GetKeyDown(KeyCode.Alpha0))
            Disable();

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

        Debug.Log("Start Drawing");
    }

    private void StopDrawing()
    {
        brush.SetActive(false);

        isDrawing = false;

        Debug.Log("Stop Drawing");
    }

    private void Draw()
    {
        if (!isDrawing)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (drawingCollider.Raycast(ray, out RaycastHit hitInfo, mainCamera.farClipPlane))
        {
            MoveBrush(hitInfo.point + drawingCamera.transform.forward);

            //lastPosition = hitInfo.point + drawingCamera.transform.forward;
        }

    }

    private const float p = 180f / Mathf.PI;
    private void MoveBrush(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(brush.transform.position, targetPosition);
        if (distance < stopDistance)
            return;

        brush.transform.position = lastPosition;
        RegenerateVertices(brush.GetComponent<MeshFilter>().mesh, distance, 0.075f);

        Vector3 direction = (targetPosition - brush.transform.position).normalized;
        Vector3 newEulerRotation = new(0f, 0f, p * Mathf.Asin(direction.y));
        brush.transform.eulerAngles = newEulerRotation;
        Debug.LogWarning(lastPosition + " : " + targetPosition);
        lastPosition = targetPosition;
        //float brushTranslation = BrushSize / brushSizeCoefficient;

        //Vector3 newPosition = brush.transform.position + brushTranslation * direction;

        //float distance1 = (newPosition - targetPosition).magnitude;

        //if (brushTranslation > distance1)
        //    newPosition = targetPosition;

        //brush.transform.position = newPosition;
    }

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
