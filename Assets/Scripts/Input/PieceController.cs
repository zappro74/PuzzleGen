using System.Collections;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    [Header("Script Connections")]
    public AudioManager audioManager;
    public SnappingMethods snapManager;
    public GameModeController modeController;
    public AnimationController animationController;
    public Boundaries playBoundaries;

    [Header("Drag Settings")]
    [SerializeField] private float dragSmoothTime = 0.08f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationStep = 90f;
    [SerializeField] private float rotationDuration = 0.15f;
    private bool isRotating = false;

    private Vector3 lastDragPosition;
    private Transform selection;
    private Renderer render;
    private int order = 1;
    private Vector3 dragVelocity;

    public bool TryPickup(Vector3 mousePosition)
    {
        if(audioManager != null) audioManager.PlayGrabSound();
        var topPiece = GrabPiece(mousePosition);

        if (topPiece.collider != null)
        {
            selection = GetRoot(topPiece.transform);
            render = topPiece.transform.GetComponent<Renderer>();

            SnapRotation(selection);

            order = GetHighestSortingOrder() + 1;
            dragVelocity = Vector3.zero;

            var renderers = selection.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sortingOrder = order + 1;
            }

            lastDragPosition = selection.position;

            audioManager.StartDragAudio();
            return true;
        }
        return false;
    }
    public void TryRotate(Vector3 mousePosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (isRotating)
        {
            return;
        }
        if (selection != null)
        {
            StartCoroutine(animationController.RotationAnimation(selection, -rotationStep));
            return; 
        }

        if (hit.collider == null || !hit.collider.CompareTag("Piece"))
        {
            return;
        }

        Transform root = GetRoot(hit.transform);

        StartCoroutine(animationController.RotationAnimation(root, -rotationStep));
    }

    public RaycastHit2D GrabPiece(Vector3 mousePosition)
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
    public void DragPiece(Vector3 mousePosition)
    {
        if (selection == null || render == null) return;

        Vector3 groupCenter = GetGroupCenter(selection);
        Vector3 centerToRoot = selection.position - groupCenter;
        Vector3 targetPosition = mousePosition + centerToRoot;

        targetPosition.z = selection.position.z;
        targetPosition = WorldBoundaries(targetPosition);

        int groupSize = selection.GetComponentsInChildren<PuzzlePiece>().Length;
        float adjustedSmoothTime = dragSmoothTime * (1f + ((groupSize - 1) * 0.15f));

        selection.position = Vector3.SmoothDamp(selection.position, targetPosition, ref dragVelocity, adjustedSmoothTime);
        
        float speed = (selection.position - lastDragPosition).magnitude / Time.deltaTime;

        if(audioManager != null) audioManager.UpdateDragAudio(speed, groupSize, false);

        lastDragPosition = selection.position;

        if (snapManager != null && modeController.currentGameMode == GameMode.Easy)
        {
            bool didSnap = snapManager.TryAutoSnap(selection);

            if (didSnap)
            {
                selection = null;
                render = null;
                dragVelocity = Vector3.zero;
                
                audioManager.StopDragAudio();
                return;
            }
        }
    }
    public bool IsHoldingPiece()
    {
        return selection != null;
    }
    public void ReleasePiece()
    {
        if (selection == null) return;

        if (snapManager != null && modeController.currentGameMode != GameMode.Easy)
        {
            snapManager.TrySnap(selection);
        }

        if (audioManager != null) audioManager.StopDragAudio();

        selection = null;
        render = null;
        dragVelocity = Vector3.zero;
    }
    private Vector3 WorldBoundaries(Vector3 movement)
    {
        var center = movement - (selection.position - GetGroupCenter(selection));
        var boundaries = playBoundaries.boundaries;

        var left = (-boundaries.x / 2f) + render.bounds.extents.x;
        var right = (boundaries.x / 2f) - render.bounds.extents.x;
        var bottom = (-boundaries.y / 2f) + render.bounds.extents.y;
        var top = (boundaries.y / 2f) - render.bounds.extents.y;

        center.x = Mathf.Clamp(center.x, left, right);
        center.y = Mathf.Clamp(center.y, bottom, top);

        return center + (selection.position - GetGroupCenter(selection));    
    }
    public void SnapRotation(Transform target)
    {
        float angle = GetZAngle(target.rotation);
        float snapped = Mathf.Round(angle / rotationStep) * rotationStep;
        target.rotation = Quaternion.AngleAxis(snapped, Vector3.forward);
    }
    private float GetZAngle(Quaternion rotation)
    {
        Vector3 right = rotation * Vector3.right;
        return Mathf.Atan2(right.y, right.x) * Mathf.Rad2Deg;
    }
    public Vector3 GetGroupCenter(Transform group)
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
    public static Transform GetRoot(Transform piece)
    {
        while (piece.parent != null && piece.parent.CompareTag("Piece"))
        {
            piece = piece.parent;
        }
        return piece;
    }

    private int GetHighestSortingOrder()
    {
        int highest = int.MinValue;

        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);

        foreach (Renderer renderer in renderers)
        {
            if (renderer.CompareTag("Piece"))
            {
                highest = Mathf.Max(highest, renderer.sortingOrder);
            }
        }

        return highest == int.MinValue ? 0 : highest;
    }
    public IEnumerator SimulateDragToPosition(Transform root, Vector3 targetPosition, float targetRotation, float duration)
    {
        Vector3 startPosition = root.position;
        Quaternion startRotation = root.rotation;
        Quaternion endRotation = Quaternion.Euler(0f, 0f, targetRotation);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            root.position = Vector3.Lerp(startPosition, targetPosition, t);
            root.rotation = Quaternion.Lerp(startRotation, endRotation, t);

            yield return null;
        }

        root.position = targetPosition;
        root.rotation = endRotation;
    }
}
