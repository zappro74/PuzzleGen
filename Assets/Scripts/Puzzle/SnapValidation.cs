using UnityEngine;
public class SnapValidation
{
    //Snap Tolerance and rotation validation still needs implementation, can include when needed.

    public bool CanSnap(PieceData a, PieceData b, float snapTolerance)
    {
        NeighborDetection neighborDetection = new NeighborDetection;

        if(!neighborDetection.AreNeighbors(a,b))
        {
            return false;
        }

        PieceDirection direction = neighborDetection.GetRelativeDirection(a, b);
        EdgeType edgeA = EdgeType.None;
        EdgeType edgeB = EdgeType.None;

        switch(direction)
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

        if(a == EdgeType.Extruded && b == EdgeType.Intruded || a == EdgeType.Intruded && b == EdgeType.Extruded)
        {
            return true;
        }
        else if(a == EdgeType.Flat && b == EdgeType.Flat)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}