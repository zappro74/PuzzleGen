using UnityEngine;

public class NeighborDetection
{
    public bool AreNeighbors(PieceData a, PieceData b)
    {
        int rowDiff = Mathf.Abs(a.Row - b.Row);
        int columnDiff = Mathf.Abs(a.Column - b.Column);

        if(rowDiff + columnDiff == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public Direction GetRelativeDirection(PieceData a, PieceData b)
    {
        if(a.Column == b.Column)
        {
            if(a.Row - 1 == b.Row)
            {
                return PieceDirection.Top
            }
            if (a.Row + 1 == b.Row)
            {
                return PieceDirection.Bottom
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
        if(AreNeighbors == false)
        {
            return PieceDirection.None;
        }

        return PieceDirection.None;
    }
}