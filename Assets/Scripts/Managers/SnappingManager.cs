using UnityEngine;

public class SnappingManager : MonoBehaviour
{
    [Header("Snapping Settings")]
    public float snappingTolerance = 0.5f;

    public ConnectionSystem connectionSystem;
    private SnapValidation snapValidator = new SnapValidation();

    public void TrySnap(Transform pieceGroup)
    {
        if (connectionSystem == null)
        {
            Debug.LogWarning("Missing connection system.");
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

                if (snapValidator.CanSnap(groupPiece.Data, piece.Data, snappingTolerance))
                {    
    
                    var snappingPosition = (Vector2)piece.transform.position + ((Vector2)groupPiece.SolvedPosition - (Vector2)piece.SolvedPosition);
                    var distance = Vector2.Distance(groupPiece.transform.position, snappingPosition);

                    if (distance <= snappingTolerance)
                    {
                    
                        pieceGroup.position += (Vector3)snappingPosition - groupPiece.transform.position;;
                        pieceGroup.SetParent(GetRoot(piece.transform));
                        connectionSystem.AddConnection(groupPiece.Data, piece.Data);

                        Debug.Log($"Snapped Piece {groupPiece.Data.Id} to Piece {piece.Data.Id}");
                        return;
                    }
                }
            }
        }
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
