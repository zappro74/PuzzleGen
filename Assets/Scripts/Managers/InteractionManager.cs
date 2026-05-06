using UnityEngine;
using UnityEngine.InputSystem;
using SimpleFileBrowser;

public class InteractionManager : MonoBehaviour
{
    [Header("Script Connections")]
    public SnappingManager snapManager;
    public MenuController menuController;

    [Header("Drag Settings")]
    [SerializeField] private float dragSmoothTime = 0.08f;

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float zoomSpeed = 0.2f; // smaller is typically better here

    [Header("Boundary Settings")]
    public Vector2 boundaries = new Vector2(30f, 20f);
    [SerializeField] private Color boundaryColor = Color.red;
    [SerializeField] private float boundaryThickness = 0.15f;

    private Camera gameCamera;
    private Transform selection;
    private Vector3 offset;
    private Renderer render;
    private int order = 1;
    private Vector3 dragTargetPosition;
    private Vector3 dragVelocity;
    private Vector3 origin;
    private bool isPanning = false;
  
    void Start()
    {
        gameCamera = Camera.main;
        BoundaryLines();
    }
    void Update()
    {
        if (Mouse.current == null || gameCamera == null || FileBrowser.IsOpen || menuController.MenuCheck())
        {
            return;
        }

        var leftButton = Mouse.current.leftButton;
        var y = Mouse.current.scroll.ReadValue().y;
        Vector3 mousePosition = gameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0f;

        if (Mathf.Abs(y) > 0.01f)
        {
            ZoomCamera(y, mousePosition);
        }

        if (leftButton.wasPressedThisFrame)
        {
            TryPickup(mousePosition);

            if (selection == null)
            {
                isPanning = true;
                origin = mousePosition;
            }
        }
        else if (leftButton.isPressed)
        {
            if (selection != null && render != null) 
            {
                Vector3 groupCenter = GetGroupCenter(selection);

                Vector3 centerToRoot = selection.position - groupCenter;
                Vector3 targetPosition = mousePosition + centerToRoot;

                targetPosition.z = selection.position.z;

                selection.position = Vector3.SmoothDamp(selection.position, targetPosition, ref dragVelocity, dragSmoothTime);
            }
            else if (isPanning)
            {
                PanCamera(mousePosition);
            }
        }
        else if (leftButton.wasReleasedThisFrame)
        {
            if (selection != null && snapManager != null)
            {
                snapManager.TrySnap(selection);
            }
            selection = null;

            selection = null;
            render = null;
            dragVelocity = Vector3.zero;
            isPanning = false;
        }
    }
    private void ZoomCamera(float y, Vector3 mousePosition)
    {
        var size = Mathf.Clamp(gameCamera.orthographicSize - (y * zoomSpeed), minZoom, Mathf.Min(maxZoom, boundaries.y / 2f, boundaries.x / 2f / gameCamera.aspect));

        if (Mathf.Approximately(gameCamera.orthographicSize, size)) 
        {
            return;
        }

        gameCamera.orthographicSize = size;

        var mousePostZoom = gameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePostZoom.z = 0f;

        gameCamera.transform.position += (mousePosition - mousePostZoom);

        CameraBoundaries();    
    }
    private void PanCamera(Vector3 mousePosition)
    {
        var difference = origin - mousePosition;
        difference.z = 0f;
        gameCamera.transform.position += difference;
        CameraBoundaries();
    }
    private void BoundaryLines()
    {
        var x = boundaries.x / 2f;
        var y = boundaries.y / 2f;
        var lineObject = new GameObject("BoundaryBox");
        var renderer = lineObject.AddComponent<LineRenderer>();

        lineObject.transform.SetParent(transform); 
        
        renderer.material = new Material(Shader.Find("Sprites/Default")); 
        renderer.startWidth = boundaryThickness;
        renderer.endWidth = boundaryThickness;
        renderer.sortingOrder = 500; 
        renderer.startColor = boundaryColor;
        renderer.endColor = boundaryColor;

        renderer.positionCount = 4;
        renderer.SetPositions(new Vector3[] 
        { 
            new(-x, -y, 0),
            new(-x, y, 0),  
            new(x, y, 0),  
            new(x, -y, 0)   
        });
        
        renderer.loop = true;    
    }
    private RaycastHit2D GrabPiece(Vector3 mousePosition)
    {
        var pieces = Physics2D.RaycastAll(mousePosition, Vector2.zero);
        int highest = int.MinValue;
        var top = new RaycastHit2D();

        foreach (var piece in pieces)
        {
            if (piece.collider != null && piece.collider.CompareTag("Piece"))
            {
                if (piece.transform.TryGetComponent(out Renderer render) && render.sortingOrder > highest)
                {
                    highest = render.sortingOrder;
                    top = piece;
                }
            }
        }
        return top;
    }
    private void TryPickup(Vector3 mousePosition)
    {
        var topPiece = GrabPiece(mousePosition);

        if (topPiece.collider != null)
        {
            selection = GetRoot(topPiece.transform);
            render = topPiece.transform.GetComponent<Renderer>();

            order = Mathf.Max(order, render.sortingOrder) + 1;
            dragVelocity = Vector3.zero;

            var renderers = selection.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sortingOrder = order + i;
            }
        }
    }

    private Vector3 GetGroupCenter(Transform group)
    {
        Collider2D[] colliders = group.GetComponentsInChildren<Collider2D>();

        if (colliders.Length == 0)
        {
            return group.position;
        }

        Bounds bounds = colliders[0].bounds;

        for (int i = 1; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }

        return bounds.center;
    }

    private void CameraBoundaries()
    {
        var x = Mathf.Max(0, (boundaries.x / 2f) - (gameCamera.orthographicSize * gameCamera.aspect));
        var y = Mathf.Max(0, (boundaries.y / 2f) - gameCamera.orthographicSize);
        var cameraPosition = gameCamera.transform.position;

        cameraPosition.x = Mathf.Clamp(cameraPosition.x, -x, x);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, -y, y);

        gameCamera.transform.position = cameraPosition;
    }
    private Vector3 WorldBoundaries(Vector3 movement)
    {
        var left = -boundaries.x / 2f;
        var right = boundaries.x / 2f;
        var bottom = -boundaries.y / 2f;
        var top = boundaries.y / 2f;

        movement.x = Mathf.Clamp(movement.x, left, right - render.bounds.size.x);
        movement.y = Mathf.Clamp(movement.y, bottom, top - render.bounds.size.y);

        return movement;    
    }
    private Transform GetRoot(Transform piece)
    {
        while (piece.parent != null && piece.parent.CompareTag("Piece"))
        {
            piece = piece.parent;
        }

        return piece;
    }
}