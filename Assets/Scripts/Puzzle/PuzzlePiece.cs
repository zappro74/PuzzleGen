using System.Data.Common;
using UnityEngine;

public class PuzzlePiece
{
    public PieceData Data { get; private set; }

    public void Initialize(PieceData data)
    {
        Data = data;
    }
}
