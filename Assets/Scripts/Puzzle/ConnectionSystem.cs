using Systems.Collections.Generic;
using UnityEngine;

public class ConnectionSystem
{
    public class PieceConnection
    {
        public int PieceAId;
        public int PieceBId;
        public PieceDirection Direction;

        public PieceConnection(int aId, int bId, PieceDirection direction)
        {
            PieceAId = aId;
            PieceBId = bId;
            Direction = direction;
        }
    }
    public bool PiecesMatch(int aId, int bId)
    {
        return (PieceAId == aId && PieceBId == bId || PieceAId == bId && PieceBId == aId);
    }

    List<PieceConnection> connections = new List<PieceConnection>();
    public bool HasConnection(PieceData a, PieceData b)
    {
        for(int i = 0; i < connections.Length; i++)
        {
            if (PiecesMatch(a.Id, b.Id)) return true;
        }
        return false;
    }

    NeighborDetection neighbors = new NeighborDetection();

    public void AddConnection(PieceData a, PieceData b)
    {
        if (a == null || b == null || a == b)
        {
            return;
        }

        if (HasConnection(a, b)) return;

        PieceDirection direction = neighbors.GetRelativeDirection(a, b);

        connections.Add(new PieceConnection(a.Id, b.Id, direction));
    }
}