using System.Collections.Generic;
using UnityEngine;

public class GroupSystem
{
    private Dictionary<int, List<PieceData>> groups = new Dictionary<int, List<PieceData>>();
   
    public void Initialize(List<PieceData> pieces)
    {
        groups.Clear();
        foreach(var piece in pieces)
        {
            groups[piece.GroupId] = new List<PieceData> {piece};
        }
    }

    public List<PieceData> GetGroup(PieceData piece)
    {
        if (groups.ContainsKey(piece.GroupId))
        {
            return groups[piece.GroupId];
        }

        return null;
    }
    
    public List<PieceData> GetGroupMembers(int groupId)
    {
        if (groups.ContainsKey(groupId))
        {
            return groups[groupId];
        }

        return null;
    }

    public void MergeGroups(PieceData a, PieceData b)
    {
        var groupAId = a.GroupId;
        var groupBId = b.GroupId;

        if(groupAId == groupBId)
        {
            return;
        }

        List<PieceData> groupA = groups[groupAId];
        List<PieceData> groupB = groups[groupBId];

        foreach(var piece in groupB)
        {
            piece.GroupId = groupAId;
            groupA.Add(piece);
        }

        groups.Remove(groupBId);
    }

    public bool IsPuzzleComplete()
    {
        return groups.Count == 1;
    }
}