using UnityEngine;

public class PieceController : MonoBehaviour
{
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
    
    private Vector3 WorldBoundaries(Vector3 movement)
    {
        var center = movement - (selection.position - GetGroupCenter(selection));

        var left = (-boundaries.x / 2f) + render.bounds.extents.x;
        var right = (boundaries.x / 2f) - render.bounds.extents.x;
        var bottom = (-boundaries.y / 2f) + render.bounds.extents.y;
        var top = (boundaries.y / 2f) - render.bounds.extents.y;

        center.x = Mathf.Clamp(center.x, left, right);
        center.y = Mathf.Clamp(center.y, bottom, top);

        return center + (selection.position - GetGroupCenter(selection));    
    }
}
