using UnityEngine;

[System.Serializable]
public class PieceData
{
    public int Id { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }

    public EdgeType TopEdge { get; set; }
    public EdgeType RightEdge { get; set; }
    public EdgeType BottomEdge { get; set; }
    public EdgeType LeftEdge { get; set; }

    public Vector2 Position { get; set; }
    public float Rotation { get; set; }

    public PieceData(int id, int row, int column)
    {
        Id = id;
        Row = row;
        Column = column;

        TopEdge = EdgeType.Flat;
        RightEdge = EdgeType.Flat;
        BottomEdge = EdgeType.Flat;
        LeftEdge = EdgeType.Flat;

        Position = Vector2.zero;
        Rotation = 0f;

    }
}