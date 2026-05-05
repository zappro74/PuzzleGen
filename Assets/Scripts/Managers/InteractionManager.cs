using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    [Header("Script Connections")]
    public SnappingManager snapManager;

    [Header("Drag")]
    [SerializeField] private float dragSmoothTime = 0.05f;

    private Camera gameCamera;
    private Transform selection;
    private Vector3 offset;
    private Renderer render;
    private int order = 1;

    private Vector3 dragTargetPosition;
    private Vector3 dragVelocity;
  
    void Start()
    {
        gameCamera = Camera.main;
    }
    void Update()
    {
        if (Mouse.current == null || gameCamera == null)
        {
            return;
        }

        var leftButton = Mouse.current.leftButton;
        Vector3 mousePosition = gameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0f;

        if (leftButton.wasPressedThisFrame)
        {
            TryPickup(mousePosition);
        }

        if (leftButton.isPressed && selection != null && render != null)
        {
            Vector3 movement = mousePosition + offset;
            dragTargetPosition = ScreenBoundaries(movement);

            selection.position = Vector3.SmoothDamp(selection.position, dragTargetPosition, ref dragVelocity, dragSmoothTime);
        }

        if (leftButton.wasReleasedThisFrame)
        {
            if (selection != null && snapManager != null)
            {
                snapManager.TrySnap(selection);
            }
            selection = null;
        }
    }
    private RaycastHit2D GrabPiece(Vector3 mousePosition)
    {
        var hitPieces = Physics2D.RaycastAll(mousePosition, Vector2.zero);
        int highestOrder = int.MinValue;
        var topPiece = new RaycastHit2D();

        foreach (var piece in hitPieces)
        {
            if (piece.collider != null && piece.collider.CompareTag("Piece"))
            {
                var render = piece.transform.GetComponent<Renderer>();

                if (render != null && render.sortingOrder > highestOrder)
                {
                    highestOrder = render.sortingOrder;
                    topPiece = piece;
                }
            }
        }
        return topPiece;
    }
    private void TryPickup(Vector3 mousePosition)
    {
        var topPiece = GrabPiece(mousePosition);

        if (topPiece.collider != null)
        {
            selection = GetRoot(topPiece.transform);
            offset = selection.position - mousePosition;
            order++;

            var renderers = selection.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                renderer.sortingOrder = order;
            }

            render = topPiece.transform.GetComponent<Renderer>();

        }
    }
    private Vector3 ScreenBoundaries(Vector3 movement)
    {
        var screenHeight = gameCamera.orthographicSize;
        var screenWidth = screenHeight * gameCamera.aspect;
        var cameraPosition = gameCamera.transform.position;
        var left = cameraPosition.x - screenWidth;
        var right = cameraPosition.x + screenWidth;
        var bottom = cameraPosition.y - screenHeight;
        var top = cameraPosition.y + screenHeight;

        movement.x = Mathf.Clamp(movement.x, left, right - render.bounds.size.x);
        movement.y = Mathf.Clamp(movement.y, bottom, top - render.bounds.size.y);

        return movement;
    }
    private Transform GetRoot(Transform piece)
    {
        while (piece.parent != null && piece.parent.GetComponent<PuzzlePiece>() != null)
        {
            piece = piece.parent;
        }

        return piece;
    }
}