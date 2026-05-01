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

        puzzleConfig.pieceConfig.pieceWidth = imageWidth / puzzleConfig.columns;
        puzzleConfig.pieceConfig.pieceHeight = imageHeight / puzzleConfig.rows;

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

        List<GameObject> createdPieces = new List<GameObject>();

        foreach (PieceData pieceData in pieceDataList)
        {
            GameObject pieceObject = pieceFactory.CreatePiece(pieceData, puzzleConfig);

            pieceObject.transform.SetParent(puzzleParent.transform);
            pieceObject.transform.localPosition = GetPiecePosition(pieceData, puzzleConfig, pieceWidth, pieceHeight);

            createdPieces.Add(pieceObject);
        }
        
        return createdPieces;
    }

    private Vector3 GetPiecePosition(PieceData pieceData, PuzzleConfig config, float pieceWidth, float pieceHeight)
    {
        float puzzleWidth = config.columns * pieceWidth;
        float puzzleHeight = config.rows * pieceHeight;

        float x = pieceData.Column * pieceWidth - puzzleWidth / 2f;
        float y = -pieceData.Row * pieceHeight + puzzleHeight / 2f - pieceHeight;

        return new Vector3(x, y, -1f);
    }
}
