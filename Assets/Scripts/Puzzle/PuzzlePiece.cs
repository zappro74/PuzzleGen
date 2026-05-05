using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    public PieceData Data { get; private set; }
    public Vector3 SolvedPosition { get; private set; }

    public void Initialize(PieceData pieceData)
    {
        Data = pieceData;
        SolvedPosition = transform.position;
    }
    public void UpdatePosition()
    {
        SolvedPosition = transform.position;
    }
}
