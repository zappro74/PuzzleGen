using System.Collections.Generic;
using UnityEngine;

public class SnappingManager : MonoBehaviour
{    
    [Header("Animation Settings")]
    public float snapSpeed = 0.25f; // the speed value of snapping
    public AnimationCurve snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // the curve inside the inspector that controls the snapping curve

    public bool IsAnimating { get; private set; } = false;

    public struct SnapPairs
    {
        public PuzzlePiece draggedPiece;
        public PuzzlePiece targetPiece;
    }

    public List<SnapPairs> ScanConnections(Transform pieceGroup, Transform targetRoot, PuzzlePiece groupPiece, PuzzlePiece piece)
    {
        var scanConnections = new List<SnapPairs>();

        foreach (var a in pieceGroup.GetComponentsInChildren<PuzzlePiece>())
        {
            foreach (var b in targetRoot.GetComponentsInChildren<PuzzlePiece>())
            {
                var distance = Vector2.Distance(a.SolvedPosition, b.SolvedPosition);
                
                if (distance > 0.1f && distance <= Vector2.Distance(groupPiece.SolvedPosition, piece.SolvedPosition) * 1.2f)
                {
                    var direction = (Vector2)a.SolvedPosition - (Vector2)b.SolvedPosition;
                    if (Mathf.Abs(direction.x) < 0.1f || Mathf.Abs(direction.y) < 0.1f)
                    {
                        var connection = new SnapPairs { draggedPiece = a, targetPiece = b };
                        scanConnections.Add(connection);
                    }
                }
            }
        }
        
        return scanConnections;
    }

    public bool IsValidSnapRotation(Transform pieceGroup, Transform targetPiece)
    {
        float angleA = Mathf.Round(pieceGroup.eulerAngles.z);
        float angleB = Mathf.Round(targetPiece.eulerAngles.z);

        float difference = Mathf.Abs(Mathf.DeltaAngle(angleA, angleB));

        return difference <= 2f;
    }
}
