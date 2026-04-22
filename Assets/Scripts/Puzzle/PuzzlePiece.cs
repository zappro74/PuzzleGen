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

    public GameObject CreatePiece(PieceData pieceData)
    {
        GameObject pieceObject = new GameObject($"Piece_{pieceData.Id}");
        PuzzlePiece puzzlePiece = pieceObject.AddComponent<PuzzlePiece>();

        puzzlePiece.Initialize(pieceData);

        return pieceObject;
    }

    public void CreateAllPieces(List<PieceData> pieces)
    {
        foreach (PieceData pieceData in pieces)
        {
            CreatePiece(pieceData);
        }
    }
}
