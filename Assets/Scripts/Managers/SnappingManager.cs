using UnityEngine;

public class SnappingManager : MonoBehaviour
{
    [Header("Snapping Settings")]
    public float snappingTolerance = 0.5f;

    public ConnectionSystem connectionSystem;
    private SnapValidation snapValidator = new SnapValidation();

    public void TrySnap(Transform pieceGroup)
    {
        var activePieces = pieceGroup.GetComponentsInChildren<PuzzlePiece>();
        var allPieces = FindObjectsByType<PuzzlePiece>();

        foreach (var activePiece in activePieces)
        {
            foreach (var piece in allPieces)
            {
                if (activePiece == piece || activePiece.transform.root == piece.transform.root)
                {
                    continue;
                }

                if (snapValidator.CanSnap(activePiece.Data, piece.Data, snappingTolerance))
                {
                    var offset = activePiece.SolvedPosition - piece.SolvedPosition;
                    var piecePosition = piece.transform.position + offset;

                    if (Vector3.Distance(activePiece.transform.position, piecePosition) <= snappingTolerance)
                    {
                        var snapAdjustment = piecePosition - activePiece.transform.position;

                        pieceGroup.position += snapAdjustment;
                        pieceGroup.SetParent(piece.transform.root);
                        connectionSystem.AddConnection(activePiece.Data, piece.Data);

                        Debug.Log($"Snapped Piece {activePiece.Data.Id} to Piece {piece.Data.Id}");
                        return;
                    }
                }
            }
        }
    }
}
