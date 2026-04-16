using UnityEngine;

[System.Serializable]
public class PieceData
{
    public int Id { get; set; };
    public int Row { get; set; };
    public int Column { get; set; }

    public EdgeType TopEdge { get; set; };
    public EdgeType RightEdge { get; set; };
    public EdgeType BottomEdge { get; set; };
    public EdgeType LeftEdge { get; set; };

    public vector2 Position { get; set; };
    public float Rotation { get; set; };
}
