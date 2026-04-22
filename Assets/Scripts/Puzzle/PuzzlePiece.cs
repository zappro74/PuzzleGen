using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    public PieceData Data { get; private set; }

    public void Initialize(PieceData pieceData)
    {
        Data = pieceData;
    }
}
