using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{   
    [Header("Camera Connection")]
    [SerializeField] private Camera gameCamera;
    public Boundaries playBoundaries;

    [Header("Zoom Settings")]
    [SerializeField] public float minZoom = 2f;
    [SerializeField] public float maxZoom = 15f;
    [SerializeField] public float zoomSpeed = 0.2f;

    private Vector3 origin;
    public bool isPanning { get; private set; } = false;

    public void ZoomCamera(float y, Vector3 mousePosition)
    {
        var boundaries = playBoundaries.boundaries;
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
    public void StartPanning(Vector3 startPosition)
    {
        isPanning = true;
        origin = startPosition;
    }
    public void PanCamera(Vector3 mousePosition)
    {
        var difference = origin - mousePosition;
        difference.z = 0f;
        gameCamera.transform.position += difference;
        CameraBoundaries();
    }
    public void StopPanning()
    {
        isPanning = false;
    }
    public IEnumerator LerpCameraZoom(float startSize, float targetSize, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (gameCamera == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            gameCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

            yield return null;
        }

        if (gameCamera != null)
        {
            gameCamera.orthographicSize = targetSize;
        }
    }
    public IEnumerator LerpCameraPosition(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (gameCamera == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            gameCamera.transform.position = Vector3.Lerp(from, to, t);

            yield return null;
        }

        if (gameCamera != null)
        {
            gameCamera.transform.position = to;
        }
    }
    private void CameraBoundaries()
    {
        var boundaries = playBoundaries.boundaries;
        var x = Mathf.Max(0, (boundaries.x / 2f) - (gameCamera.orthographicSize * gameCamera.aspect));
        var y = Mathf.Max(0, (boundaries.y / 2f) - gameCamera.orthographicSize);
        var cameraPosition = gameCamera.transform.position;

        cameraPosition.x = Mathf.Clamp(cameraPosition.x, -x, x);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, -y, y);

        gameCamera.transform.position = cameraPosition;
    }
}
