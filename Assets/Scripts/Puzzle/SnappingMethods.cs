using System.Collections.Generic;
using UnityEngine;


public class SnappingMethods : MonoBehaviour
{
    [Header("Script Connections")]
    public ConnectionSystem connectionSystem;
    public GameStateManager gameStateManager;
    public AnimationController animator;
    public SnappingManager snappingManager;

    [Header("Snapping Settings")]
    public float snappingTolerance = 0.5f;

    private SnapValidation snapValidator = new SnapValidation();
    public struct SnapPairs { public PuzzlePiece draggedPiece; public PuzzlePiece targetPiece; }

    public bool TrySnap(Transform pieceGroup)
    {
        if (connectionSystem == null)
        {
            Debug.LogWarning("Missing connection system.");
            return false;
        }
        if (gameStateManager.elapsedTime < 2f)
        {
            return false;
        }
        var groupedPieces = pieceGroup.GetComponentsInChildren<PuzzlePiece>();
        var allPieces = FindObjectsByType<PuzzlePiece>(FindObjectsInactive.Exclude);

        foreach (var groupPiece in groupedPieces)
        {
            foreach (var piece in allPieces)
            {
                if (groupPiece == piece || groupPiece.Data.GroupId == piece.Data.GroupId)
                {
                    continue;
                }

                if (snapValidator.CanSnap(groupPiece.Data, piece.Data, groupPiece.transform, piece.transform, snappingTolerance))
                {    
                    Vector2 solvedOffset = (Vector2)groupPiece.SolvedPosition - (Vector2)piece.SolvedPosition;
                    Quaternion rotation = PieceController.GetRoot(piece.transform).rotation;
                    Vector2 rotatedOffset = rotation * solvedOffset;
                    Vector2 snappingPosition = (Vector2)piece.transform.position + rotatedOffset;
                    var distance = Vector2.Distance(groupPiece.transform.position, snappingPosition);

                    if (distance <= snappingTolerance)
                    {
                        Vector3 adjustment = (Vector3)snappingPosition - groupPiece.transform.position;
                        Transform targetRoot = PieceController.GetRoot(piece.transform);
                        connectionSystem.AddConnection(groupPiece.Data, piece.Data);

                        Vector3 startWorldPosition = pieceGroup.position;
                        pieceGroup.position += adjustment;

                        Vector3 endWorldPosition = pieceGroup.position;
                        pieceGroup.position = startWorldPosition;

                        var scanConnections = snappingManager.ScanConnections(pieceGroup, targetRoot, groupPiece, piece);
                        StartCoroutine(animator.Animate(pieceGroup, targetRoot, startWorldPosition, endWorldPosition, pieceGroup.rotation, rotation, scanConnections));

                        Debug.Log($"Snapped Piece {groupPiece.Data.Id} to Piece {piece.Data.Id}");

                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool TryAutoSnap(Transform pieceGroup)
    {
        if (connectionSystem == null)
        {
            Debug.LogWarning("Missing connection system.");
            return false;
        }
        if (gameStateManager.elapsedTime < 2f)
        {
            return false;
        }

        PuzzlePiece[] groupedPieces = pieceGroup.GetComponentsInChildren<PuzzlePiece>();
        PuzzlePiece[] allPieces = FindObjectsByType<PuzzlePiece>(FindObjectsInactive.Exclude);

        foreach (PuzzlePiece groupPiece in groupedPieces)
        {
            foreach (PuzzlePiece piece in allPieces)
            {
                if (groupPiece == piece || groupPiece.Data.GroupId == piece.Data.GroupId)
                {
                    continue;
                }

                if (snapValidator.CanSnap(groupPiece.Data, piece.Data, groupPiece.transform, piece.transform, snappingTolerance))
                {
                    Vector2 solvedOffset = (Vector2)groupPiece.SolvedPosition - (Vector2)piece.SolvedPosition;
                    Quaternion rotation = PieceController.GetRoot(piece.transform).rotation;
                    Vector2 rotatedOffset = rotation * solvedOffset;
                    Vector2 snappingPosition = (Vector2)piece.transform.position + rotatedOffset;
                    float distance = Vector2.Distance(groupPiece.transform.position, snappingPosition);

                    if (distance <= snappingTolerance)
                    {
                        if (!snappingManager.IsValidSnapRotation(pieceGroup, piece.transform))
                        {
                            continue;
                        }

                        Vector3 adjustment = (Vector3)snappingPosition - groupPiece.transform.position;
                        Transform targetRoot = PieceController.GetRoot(piece.transform);
                        connectionSystem.AddConnection(groupPiece.Data, piece.Data);

                        Vector3 startWorldPosition = pieceGroup.position;
                        pieceGroup.position += adjustment;

                        Vector3 endWorldPosition = pieceGroup.position;
                        pieceGroup.position = startWorldPosition;

                        var scanConnections = snappingManager.ScanConnections(pieceGroup, targetRoot, groupPiece, piece);
                        StartCoroutine(animator.Animate(pieceGroup, targetRoot, startWorldPosition, endWorldPosition, pieceGroup.rotation, rotation, scanConnections));

                        Debug.Log($"Auto snapped Piece {groupPiece.Data.Id} to Piece {piece.Data.Id}");

                        return true;
                    }
                }
            }
        }
        return false;
    }
}