using UnityEngine;

public class SnapValidation
{
    public bool CanSnap(PieceData a, PieceData b, Transform aTransform, Transform bTransform, float snapTolerance)
    {
        NeighborDetection neighborDetection = new NeighborDetection();

        if (!neighborDetection.AreNeighbors(a, b))
        {
            return false;
        }

        if (!SameRotation(aTransform, bTransform))
        {
            return false;
        }

        PieceDirection direction = neighborDetection.GetRelativeDirection(a, b);

        EdgeType edgeA;
        EdgeType edgeB;

        switch (direction)
        {
            case PieceDirection.Right:
                edgeA = a.RightEdge;
                edgeB = b.LeftEdge;
                break;

            case PieceDirection.Left:
                edgeA = a.LeftEdge;
                edgeB = b.RightEdge;
                break;

            case PieceDirection.Top:
                edgeA = a.TopEdge;
                edgeB = b.BottomEdge;
                break;

            case PieceDirection.Bottom:
                edgeA = a.BottomEdge;
                edgeB = b.TopEdge;
                break;

            default:
                return false;
        }

        return (edgeA == EdgeType.Extruded && edgeB == EdgeType.Intruded) || (edgeA == EdgeType.Intruded && edgeB == EdgeType.Extruded) || (edgeA == EdgeType.Flat && edgeB == EdgeType.Flat);
    }

    private bool SameRotation(Transform a, Transform b)
    {
        Transform aRoot = GetRoot(a);
        Transform bRoot = GetRoot(b);

        int aRotation = NormalizeRotation(aRoot.eulerAngles.z);
        int bRotation = NormalizeRotation(bRoot.eulerAngles.z);

        return aRotation == bRotation;
    }

    private int NormalizeRotation(float zRotation)
    {
        int rounded = Mathf.RoundToInt(zRotation);

        rounded = ((rounded % 360) + 360) % 360;

        return rounded;
    }

    private Transform GetRoot(Transform piece)
    {
        while (piece.parent != null && piece.parent.CompareTag("Piece"))
        {
            piece = piece.parent;
        }

        return piece;
    }
}