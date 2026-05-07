using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using SimpleFileBrowser;

public class InteractionManager : MonoBehaviour
{
    [Header("Script Connections")]
    public SnappingManager snapManager;
    public MenuController menuController;

    [Header("UI Connections")]
    public GameObject centerPanel;

    [Header("Drag Settings")]
    [SerializeField] private float dragSmoothTime = 0.08f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationStep = 90f;

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float zoomSpeed = 0.2f; // smaller is typically better here

    [Header("Boundary Settings")]
    public Vector2 boundaries = new Vector2(30f, 20f);
    [SerializeField] private Color boundaryColor = Color.red;
    [SerializeField] private float boundaryThickness = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioSource dragAudio;
    [SerializeField] private AudioSource grabAudioSource;
    [SerializeField] private AudioClip[] grabSounds;
    [SerializeField] private float maxDragSpeed = 10f;
    [SerializeField] private float maxDragVolume = .8f;
    [SerializeField] private float minPitch = 0.5f;
    [SerializeField] private float maxPitch = 1.3f;

    private Vector3 lastDragPosition;
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
        if (Mouse.current == null || gameCamera == null || FileBrowser.IsOpen || menuController.MenuCheck() || centerPanel.gameObject.activeInHierarchy)
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

        if (rightButton.wasPressedThisFrame)
        {
            TryRotate(mousePosition);
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
                
                float speed = (selection.position - lastDragPosition).magnitude / Time.deltaTime;

                int groupSize = selection.GetComponentsInChildren<Collider2D>().Length;

                bool isSnapping = false;

                UpdateDragAudio(speed, groupSize, isSnapping);

                lastDragPosition = selection.position;
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

            StartCoroutine(FadeOutDragAudio());

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
        PlayGrabSound();
        var topPiece = GrabPiece(mousePosition);

        if (topPiece.collider != null)
        {
            selection = GetRoot(topPiece.transform);
            render = topPiece.transform.GetComponent<Renderer>();

            SnapRotation(selection);

            order = Mathf.Max(order, render.sortingOrder) + 1;
            dragVelocity = Vector3.zero;

            var renderers = selection.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sortingOrder = order + i;
            }

            lastDragPosition = selection.position;

            dragAudio.loop = true;
            dragAudio.volume = 0f;

            if (!dragAudio.isPlaying)
            {
                dragAudio.Play();
            }
        }
    }
    private void SnapRotation(Transform target)
    {
        float angle = GetZAngle(target.rotation);
        float snapped = Mathf.Round(angle / rotationStep) * rotationStep;
        target.rotation = Quaternion.AngleAxis(snapped, Vector3.forward);
    }
    private float GetZAngle(Quaternion rotation)
    {
        Vector3 right = rotation * Vector3.right;
        return Math.Atan2(right.y, right.x) * Mathf.Rad2Deg;
    }
    private void TryRotate(Vector3 mousePosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider == null || !hit.collider.CompareTag("Piece"))
        {
            return;
        }

        Transform root = GetRoot(hit.transform);
        root.rotation *= Quaternion.AngleAxis(-rotationStep, Vector3.forward);

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

    private void UpdateDragAudio(float speed, int groupSize, bool isSnapping)
    {
        float speed01 = Mathf.Clamp01(speed / maxDragSpeed);

        float groupVolumeBoost = Mathf.Clamp01(groupSize / 10f) * 0.2f;

        float targetVolume = (speed01 * maxDragVolume) + groupVolumeBoost;

        float groupPitchDrop = Mathf.Clamp01(groupSize / 10f) * 0.15f;

        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speed01) - groupPitchDrop;

        if (isSnapping)
        {
            targetVolume *= 0.5f;
            targetPitch *= 1.1f;
        }

        dragAudio.volume = Mathf.Lerp(dragAudio.volume, targetVolume, Time.deltaTime * 10f);

        dragAudio.pitch = Mathf.Lerp(dragAudio.pitch, targetPitch, Time.deltaTime * 10f);
    }

    private IEnumerator FadeOutDragAudio()
    {
        float startVolume = dragAudio.volume;

        while (dragAudio.volume > 0.01f)
        {
            dragAudio.volume = Mathf.Lerp(dragAudio.volume, 0f, Time.deltaTime * 12f);

            yield return null;
        }

        dragAudio.Stop();
        dragAudio.volume = 0f;
    }

    private void PlayGrabSound()
    {
        if (grabAudioSource == null || grabSounds == null || grabSounds.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, grabSounds.Length);

        grabAudioSource.volume = 4f;

        grabAudioSource.pitch = Random.Range(0.95f, 1.05f);

        grabAudioSource.PlayOneShot(grabSounds[randomIndex]);
    }
}