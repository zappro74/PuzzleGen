using UnityEngine;

public class SnapValidation
{
    public bool CanSnap(PieceData a, PieceData b, float snapTolerance)
    {
        NeighborDetection neighborDetection = new NeighborDetection();

        if (!neighborDetection.AreNeighbors(a, b))
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

        if ((edgeA == EdgeType.Extruded && edgeB == EdgeType.Intruded) ||
            (edgeA == EdgeType.Intruded && edgeB == EdgeType.Extruded))
        {
            return true;
        }

        if (edgeA == EdgeType.Flat && edgeB == EdgeType.Flat)
        {
            return true;
        }

        return false;
    }
}