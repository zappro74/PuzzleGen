using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [Header("Script Connections")]
    public PieceMerger pieceMerger;
    public ConnectionSystem connectionSystem;
    public GameStateManager gameStateManager;
    public WinManager winManager;
    public AudioManager audioManager;
    public VisualFunctions visualFunctions;
    public PieceController pieceController;

    [Header("Animation Settings")]
    public float snapSpeed = 0.25f; // the speed value of snapping
    public AnimationCurve snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // the curve inside the inspector that controls the snapping curve
    public bool IsAnimating { get; private set; } = false;
    
    [Header("Rotation Settings")]
    public float rotationDuration = 0.15f;
    public float rotationStep = 90f;
    private bool isRotating = false;

    public IEnumerator Animate(Transform pieceGroup, Transform targetRoot, Vector3 start, Vector3 end, Quaternion startRotation, Quaternion endRotation, List<SnappingManager.SnapPairs> scanConnections)
    {
        IsAnimating = true;

        float elapsedTime = 0f;

        Collider2D[] colliders = pieceGroup.GetComponentsInChildren<Collider2D>();

        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        while (elapsedTime < snapSpeed)
        {
            elapsedTime += Time.deltaTime;
            pieceGroup.position = Vector3.Lerp(start, end, snapCurve.Evaluate(elapsedTime / snapSpeed));
            pieceGroup.rotation = Quaternion.Lerp(startRotation, endRotation, snapCurve.Evaluate(elapsedTime / snapSpeed));
            yield return null;
        }

        pieceGroup.position = end;
        pieceGroup.rotation = endRotation;

        pieceMerger.MergeGroups(pieceGroup, targetRoot);

        audioManager.PlaySnapSound();

        foreach (var connection in scanConnections)
        {
            visualFunctions.SnappingParticles(connection.draggedPiece, connection.targetPiece);
        }

        visualFunctions.RestorePieceRenderers(targetRoot);

        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }

        if (connectionSystem.Groups.IsPuzzleComplete())
        {
            winManager.WinGame();
        }

        IsAnimating = false;
    }
    public IEnumerator RotationAnimation(Transform target, float totalAngle)
    {
        isRotating = true;

        float elapsed = 0f;
        float rotatedAmount = 0f;

        while (elapsed < rotationDuration)
        {
            float progress = elapsed / rotationDuration;
            float erasedProgress = Mathf.SmoothStep(0f, 1f, progress);

            float targetAngle = Mathf.Lerp(0f, totalAngle, erasedProgress);
            float angleThisFrame = targetAngle - rotatedAmount;

            Vector3 center = pieceController.GetGroupCenter(target);

            target.RotateAround(center, Vector3.forward, angleThisFrame);

            rotatedAmount = targetAngle;
            elapsed += Time.deltaTime;

            yield return null;
        }

        Vector3 finalCenter = pieceController.GetGroupCenter(target);
        target.RotateAround(finalCenter, Vector3.forward, totalAngle - rotatedAmount);

        pieceController.SnapRotation(target);

        isRotating = false;
    }
    public IEnumerator DriftToSavedPosition(Transform piece, Vector3 startPosition, Vector3 targetPosition, float targetRotationZ, float driftDuration)
    {
        Quaternion startingRotation = Quaternion.identity;
        Quaternion targetRotation   = Quaternion.Euler(0f, 0f, targetRotationZ);

        float elapsed = 0f;

        while (elapsed < driftDuration)
        {
            if (piece == null) 
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / driftDuration);
            float progressSmoothed = Mathf.SmoothStep(0f, 1f, progress);

            piece.position = Vector3.Lerp(startPosition, targetPosition, progressSmoothed);
            piece.rotation = Quaternion.Lerp(startingRotation, targetRotation, progressSmoothed);

            yield return null;
        }

        if (piece == null) 
        {
            yield break;
        }

        piece.position = targetPosition;
        piece.rotation = targetRotation;
    }
}
