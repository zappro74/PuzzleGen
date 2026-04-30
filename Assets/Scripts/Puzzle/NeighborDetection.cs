using UnityEngine;

public class NeighborDetection
{
    public bool AreNeighbors(PieceData a, PieceData b)
    {
        int rowDiff = Mathf.Abs(a.Row - b.Row);
        int columnDiff = Mathf.Abs(a.Column - b.Column);

        return(rowDiff + columnDiff == 1);
    }
    public PieceDirection GetRelativeDirection(PieceData a, PieceData b)
    {
        if (!AreNeighbors(a, b))
        {
            return PieceDirection.None;
        }
        if (a.Column == b.Column)
        {
            if(a.Row - 1 == b.Row)
            {
                return PieceDirection.Top;
            }
            if (a.Row + 1 == b.Row)
            {
                return PieceDirection.Bottom;
            }
        }
        if(a.Row == b.Row)
        {
            if(a.Column - 1 == b.Column)
            {
                return PieceDirection.Left;
            }
            if (a.Column + 1 == b.Column)
            {
                return PieceDirection.Right;
            }
        }
        
        return PieceDirection.None;
    }
}