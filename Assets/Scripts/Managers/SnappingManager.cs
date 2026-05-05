using System.Collections;
using UnityEngine;

public class SnappingManager : MonoBehaviour
{
    [Header("Snapping Settings")]
    public float snappingTolerance = 0.5f;

    [Header("Animation Settings")]
    public float snapSpeed = 0.25f; // the speed value of snapping
    public AnimationCurve snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // the curve inside the inspector that controls the snapping speed

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
                        var adjustment = (Vector3)snappingPosition - groupPiece.transform.position;

                        pieceGroup.SetParent(GetRoot(piece.transform));
                        connectionSystem.AddConnection(groupPiece.Data, piece.Data);

                        var startLocalPosition = pieceGroup.localPosition;
                        pieceGroup.position += adjustment;
                        var endLocalPosition = pieceGroup.localPosition;
                        pieceGroup.localPosition = startLocalPosition;

                        StartCoroutine(Animate(pieceGroup, startLocalPosition, endLocalPosition));

                        Debug.Log($"Snapped Piece {groupPiece.Data.Id} to Piece {piece.Data.Id}");
                        return;
                    }
                }
            }
        }
    }
    private IEnumerator Animate(Transform piece, Vector3 start, Vector3 end)
    {
        var elapsedTime = 0f;
        var colliders = piece.GetComponentsInChildren<Collider2D>();

        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        while (elapsedTime < snapSpeed)
        {
            elapsedTime += Time.deltaTime;
            piece.localPosition = Vector3.Lerp(start, end, snapCurve.Evaluate(elapsedTime / snapSpeed));
            yield return null; 
        }

        piece.localPosition = end;

        foreach (var collider in colliders)
        {
            collider.enabled = true;
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
