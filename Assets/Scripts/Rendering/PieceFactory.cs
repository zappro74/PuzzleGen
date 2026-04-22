using UnityEngine;
using System.Collections.Generic;

public class PieceFactory : MonoBehaviour
{
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
