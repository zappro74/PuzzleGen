using System.Collections.Generic;
using UnityEngine;

public class PuzzleFactory : MonoBehaviour
{
    private PieceFactory pieceFactory;
    [SerializeField] private Transform puzzleBoard;
    public List<GameObject> GeneratePuzzle(PuzzleConfig puzzleConfig, float imageWidth, float imageHeight)
    {
        if (puzzleConfig == null)
        {
            Debug.LogError("PuzzleConfig is null.");
            return new List<GameObject>();
        }

        if (puzzleConfig.pieceConfig == null)
        {
            Debug.LogError("PieceConfig is null.");
            return new List<GameObject>();
        }

        if (puzzleConfig.rows <= 0 || puzzleConfig.columns <= 0)
        {
            Debug.LogError("Rows and columns must be greater than 0.");
            return new List<GameObject>();
        }

        pieceFactory = GetComponent<PieceFactory>();

        if (pieceFactory == null)
        {
            pieceFactory = gameObject.AddComponent<PieceFactory>();
        }

        float pieceWidth = imageWidth / puzzleConfig.columns;
        float pieceHeight = imageHeight / puzzleConfig.rows;

        PuzzleGenerator generator = new PuzzleGenerator(puzzleConfig.rows, puzzleConfig.columns, puzzleConfig.generationSeed);

        List<PieceData> pieceDataList = generator.Generate();

        GameObject puzzleParent = new GameObject("Puzzle");

        puzzleParent.transform.SetParent(puzzleBoard);
        puzzleParent.transform.localPosition = Vector3.zero;

        List<GameObject> createdPieces = new List<GameObject>();

        foreach (PieceData pieceData in pieceDataList)
        {
            GameObject pieceObject = pieceFactory.CreatePiece(pieceData, puzzleConfig.pieceConfig, pieceWidth, pieceHeight);

            pieceObject.transform.SetParent(puzzleParent.transform);
            pieceObject.transform.position = GetPiecePosition(pieceData, pieceWidth, pieceHeight);

            createdPieces.Add(pieceObject);
        }
        
        return createdPieces;
    }

    private Vector3 GetPiecePosition(PieceData pieceData, float pieceWidth, float pieceHeight)
    {        
        float x = pieceData.Column  * pieceWidth;
        float y = pieceData.Row * pieceWidth;

        return new Vector3(x, y, 0f);
    }
}
