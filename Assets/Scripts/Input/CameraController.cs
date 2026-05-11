using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] public float minZoom = 2f;
    [SerializeField] public float maxZoom = 15f;
    [SerializeField] public float zoomSpeed = 0.2f;

    private Camera gameCamera;
    private Vector3 origin;
    private bool isPanning = false;
}
