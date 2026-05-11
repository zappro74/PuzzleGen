using UnityEngine;

public class SnappingMethods : MonoBehaviour
{
    public void TrySnap(Transform pieceGroup)
    {
        if (connectionSystem == null)
        {
            Debug.LogWarning("Missing connection system.");
            return;
        }
        if (gameStateManager.elapsedTime < 2f)
        {
            return;
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
                    Quaternion rotation = GetRoot(piece.transform).rotation;
                    Vector2 rotatedOffset = rotation * solvedOffset;
                    Vector2 snappingPosition = (Vector2)piece.transform.position + rotatedOffset;
                    var distance = Vector2.Distance(groupPiece.transform.position, snappingPosition);

                    if (distance <= snappingTolerance)
                    {
                        Vector3 adjustment = (Vector3)snappingPosition - groupPiece.transform.position;
                        Transform targetRoot = GetRoot(piece.transform);
                        connectionSystem.AddConnection(groupPiece.Data, piece.Data);

                        Vector3 startWorldPosition = pieceGroup.position;
                        pieceGroup.position += adjustment;

                        Vector3 endWorldPosition = pieceGroup.position;
                        pieceGroup.position = startWorldPosition;

                        var scanConnections = ScanConnections(pieceGroup, targetRoot, groupPiece, piece);
                        StartCoroutine(Animate(pieceGroup, targetRoot, startWorldPosition, endWorldPosition, pieceGroup.rotation, rotation, scanConnections));

                        Debug.Log($"Snapped Piece {groupPiece.Data.Id} to Piece {piece.Data.Id}");

                        return;
                    }
                }
            }
        }
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
                    Quaternion rotation = GetRoot(piece.transform).rotation;
                    Vector2 rotatedOffset = rotation * solvedOffset;
                    Vector2 snappingPosition = (Vector2)piece.transform.position + rotatedOffset;
                    float distance = Vector2.Distance(groupPiece.transform.position, snappingPosition);

                    if (distance <= snappingTolerance)
                    {
                        if (!IsValidSnapRotation(pieceGroup, piece.transform))
                        {
                            continue;
                        }

                        Vector3 adjustment = (Vector3)snappingPosition - groupPiece.transform.position;
                        Transform targetRoot = GetRoot(piece.transform);
                        connectionSystem.AddConnection(groupPiece.Data, piece.Data);

                        Vector3 startWorldPosition = pieceGroup.position;
                        pieceGroup.position += adjustment;

                        Vector3 endWorldPosition = pieceGroup.position;
                        pieceGroup.position = startWorldPosition;

                        var scanConnections = ScanConnections(pieceGroup, targetRoot, groupPiece, piece);
                        StartCoroutine(Animate(pieceGroup, targetRoot, startWorldPosition, endWorldPosition, pieceGroup.rotation, rotation, scanConnections));

                        Debug.Log($"Auto snapped Piece {groupPiece.Data.Id} to Piece {piece.Data.Id}");

                        return true;
                    }
                }
            }
        }
        return false;
    }
}
