using System.Collections.Generic;
using UnityEngine;

public class PuzzleBuilder : MonoBehaviour
{
    [Header("Script Connections")]
    public GameStateManager gameStateManager;
    public GameModeController modeController;
    public SnappingMethods snappingMethods;
    public AnimationController animationController;

    [Header("Puzzle Generation")]
    [SerializeField] private PuzzleFactory puzzleFactory;
    [SerializeField] private Material pieceMaterial;
    public int currentGenerationSeed;
    public int currentRows;
    public int currentColumns;

    [Header("Shuffling")]
    [SerializeField] private ExplosionShuffle explosionShuffle;
    [SerializeField] public float boardWidth = 8f;
    [SerializeField] public float boardHeight = 6f;

    private GroupSystem groupSystem;
    private ConnectionSystem connectionSystem;

    public void ClearPuzzle()
    {
        var pieces = GameObject.FindGameObjectsWithTag("Piece");

        foreach (GameObject piece in pieces)
        {
            Destroy(piece);
        }
        if (pieceMaterial != null)
        {
            pieceMaterial.mainTexture = null;
        }

        Debug.Log($"Cleared pieces from board.");
    }
    public Vector2 GetBoardSize(Texture image, float maxWidth, float maxHeight)
    {
        float imageAspect = image.width / (float)image.height;
        float boxAspect = maxWidth / maxHeight;

        if (imageAspect > boxAspect)
        {
            return new Vector2(maxWidth, maxWidth / imageAspect);
        }

        return new Vector2(maxHeight * imageAspect, maxHeight);
    }

    public void GenerateNewPuzzle()
    {
        if (gameStateManager.image == null) 
        {
            Debug.LogWarning("No image loaded while trying to generate puzzle.");
            return;
        }

        ClearPuzzle();

        Vector2 puzzleSize = GetBoardSize(gameStateManager.image, boardWidth, boardHeight);
        
        pieceMaterial.mainTexture = gameStateManager.image;

        GameModeSettings modeSettings = modeController.GetCurrentGameModeSettings();

        int rows = modeSettings.rows;
        int columns = modeSettings.columns;

        float pieceWidth = puzzleSize.x / columns;
        float pieceHeight = puzzleSize.y / rows;
        float smallestSide = Mathf.Min(pieceWidth, pieceHeight);

        int generationSeed = System.Guid.NewGuid().GetHashCode();
        
        currentGenerationSeed = generationSeed;
        currentRows = rows;
        currentColumns = columns;

        PieceConfig pieceConfig = new PieceConfig
        {
            pieceMaterial = pieceMaterial,
            pieceWidth = pieceWidth,
            pieceHeight = pieceHeight,
            tabWidth = 0.22f,
            edgeMargin = 0.1f,
            tabHeight = Mathf.Min(pieceWidth, pieceHeight) * 0.25f,
            pointsPerCurveHalf = 10
        };

        PuzzleConfig puzzleConfig = new PuzzleConfig
        {
            rows = rows,
            columns = columns,
            generationSeed = generationSeed,
            puzzleImage = gameStateManager.image,
            pieceConfig = pieceConfig
        };

        List<GameObject> pieces = puzzleFactory.GeneratePuzzle(puzzleConfig, puzzleSize.x, puzzleSize.y);

        if (explosionShuffle != null)
        {
            explosionShuffle.ExplodePieces(pieces);
        }

        var piecesData = new List<PieceData>();

        foreach (GameObject piece in pieces)
        {
            piece.tag = "Piece";

            var script = piece.GetComponent<PuzzlePiece>();

            if (script != null)
            {
                script.UpdatePosition(); 

                if (script.Data != null)
                {
                    piecesData.Add(script.Data);
                }
            }
        }

        groupSystem = new GroupSystem();
        connectionSystem = new ConnectionSystem(groupSystem);

        groupSystem.Initialize(piecesData);

        if (snappingMethods != null)
        {
            snappingMethods.connectionSystem = connectionSystem;
        }
        else
        {
            Debug.LogWarning("SnappingMethods not assigned.");
        }
        if (animationController != null) 
        {
            animationController.connectionSystem = connectionSystem;
        }
    }
}
