using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    private Dictionary<int, PuzzleGroup> _groups = new Dictionary<int, PuzzleGroup>();
    private int _totalPieceCount;
    private int _nextGroupId = 0;

    public void StartNewGame(int totalPieces)
    {
        _totalPieceCount = totalPieces;
        _groups.Clear();
        _nextGroupId = 0;
        for (int i = 0; i < totalPieces; i++)
        {
            var group = new PuzzleGroup(_nextGroupId++, i);
            _groups[group.GroupId] = group;
        }
    }

    public void OnPiecesConnected(int pieceIdA, int pieceIdB)
    {
        var groupA = FindGroupForPiece(pieceIdA);
        var groupB = FindGroupForPiece(pieceIdB);
        if (groupA == null || groupB == null || groupA.GroupId == groupB.GroupId)
            return;
        groupA.MergeFrom(groupB);
        _groups.Remove(groupB.GroupId);
        WinCondition.CheckWin(_groups, _totalPieceCount);
    }

    public int ActiveGroupCount() => _groups.Count;

    private PuzzleGroup FindGroupForPiece(int pieceId)
    {
        foreach (var group in _groups.Values)
            if (group.Contains(pieceId))
                return group;
        return null;
    }
}
