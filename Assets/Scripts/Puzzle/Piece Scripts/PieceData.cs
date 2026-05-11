using UnityEngine;

[System.Serializable]
public class PieceData
{
    public int Id;
    public int Row;
    public int Column;
    public int GroupId;

    public EdgeType TopEdge;
    public EdgeType RightEdge;
    public EdgeType BottomEdge;
    public EdgeType LeftEdge;

    public Vector2 Position;
    public float Rotation;

    public PieceData()
    {

    }

    public PieceData(int id, int row, int column, int groupId)
    {
        Id = id;
        Row = row;
        Column = column;
        GroupId = groupId;

        TopEdge = EdgeType.Flat;
        RightEdge = EdgeType.Flat;
        BottomEdge = EdgeType.Flat;
        LeftEdge = EdgeType.Flat;

        Position = Vector2.zero;
        Rotation = 0f;

    }
}
