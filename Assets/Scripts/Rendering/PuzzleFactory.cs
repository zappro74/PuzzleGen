using System.Collections.Generic;
using UnityEngine;

public class PuzzleFactory : MonoBehaviour
{
    private PieceFactory pieceFactory;

    public List<GameObject> GeneratePuzzle(int rows, int columns, int generationSeed, Material pieceMaterial, float pieceWidth, float pieceHeight, float tabHeight, float edgeMargin, float tabWidth, int pointsPerCurveHalf)
    {
        return new List<GameObject>();
    }

    private Vector3 GetPiecePosition(PieceData pieceData)
    {
        return new Vector3();
    }
}
