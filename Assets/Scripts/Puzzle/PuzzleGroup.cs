using System;
using System.Collections.Generic;

public class PuzzleGroup
{
    public int GroupId { get; private set; }
    private HashSet<int> _pieceIds = new HashSet<int>();

    public IReadOnlyCollection<int> PieceIds => _pieceIds;
    public int Count => _pieceIds.Count;

    public PuzzleGroup(int groupId, int initialPieceId)
    {
        GroupId = groupId;
        _pieceIds.Add(initialPieceId);
    }

    public void AddPiece(int pieceId) => _pieceIds.Add(pieceId);

    public void MergeFrom(PuzzleGroup other)
    {
        foreach (var id in other._pieceIds)
            _pieceIds.Add(id);
    }

    public bool Contains(int pieceId) => _pieceIds.Contains(pieceId);
}
