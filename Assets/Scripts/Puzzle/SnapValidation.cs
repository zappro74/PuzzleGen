using UnityEngine;
public class SnapValidation
{
    //Snap Tolerance and rotation validation still needs implementation, can include when needed.

    public bool CanSnap(PieceData a, PieceData b, float snapTolerance)
    {
        if(AreNeighbors == false)
        {
            return false;
        }

        PieceDirection direction = GetRelativeDirection(PieceData a, PieceData b);

        switch(direction)
        {
            case PieceDirection.Right:
                a = a.RightEdge;
                b = b.LeftEdge;
                break;
            case PieceDirection.Left:
                a = a.LeftEdge;
                b = b.RightEdge;
                break;
            case PieceDirection.Top:
                a = a.TopEdge;
                b = b.BottomEdge;
                break;
            case PieceDirection.Bottom:
                a = a.BottomEdge;
                b = b.BottomEdge;
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