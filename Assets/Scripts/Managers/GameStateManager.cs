using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.U2D;
using Unity.VisualScripting;

public enum State
{
    Inactive,
    Active,
    Paused
}
public enum GameMode
{
    Easy,
    Medium,
    Hard
}


public class GameStateManager : MonoBehaviour
{
    public State currentState;

    [Header("Puzzle Image")]
    public Texture image; 

    [Header("UI Connections")]
    public TextMeshProUGUI timer;

    [Header("Script Connections")]
    public SnappingManager snappingManager;

    private float elapsedTime = 0f;

    [Header("Puzzle Generation")]
    [SerializeField] private PuzzleFactory puzzleFactory;
    [SerializeField] private Material pieceMaterial;

    [Header("Shuffling")]
    [SerializeField] private ExplosionShuffle explosionShuffle;

    [Header("Game Modes")]
    [SerializeField] public GameMode currentGameMode = GameMode.Easy;

    [SerializeField] private GameModeSettings easyMode = new GameModeSettings
    {
        modeName = "Easy",
        rows = 4,
        columns = 4,
        allowRotation = false,
        allowShuffle = false,
        allowPieceCollision = false,
        snapTolerance = 0.6f,
        explosionForce = 10f,
        randomForce = 5f,
        torqueForce = 5f,
        shuffleDuration = 5f,
        showFullImagePreview = true,
        allowGroupBreaking = false,
        randomizeInitialRotation = false
    };

    [SerializeField] private GameModeSettings mediumMode = new GameModeSettings
    {
        modeName = "Medium",
        rows = 6,
        columns = 6,
        allowRotation = false,
        allowShuffle = true,
        allowPieceCollision = false,
        snapTolerance = 0.45f,
        explosionForce = 10f,
        randomForce = 5f,
        torqueForce = 5f,
        shuffleDuration = 5f,
        showFullImagePreview = true,
        allowGroupBreaking = false,
        randomizeInitialRotation = false
    };

    [SerializeField] private GameModeSettings hardMode = new GameModeSettings
    {
        modeName = "Hard",
        rows = 10,
        columns = 10,
        allowRotation = true,
        allowShuffle = true,
        allowPieceCollision = false,
        snapTolerance = 0.3f,
        explosionForce = 10f,
        randomForce = 5f,
        torqueForce = 5f,
        shuffleDuration = 5f,
        showFullImagePreview = true,
        allowGroupBreaking = true,
        randomizeInitialRotation = true
    };

    public void StartGame()
    {
        // Runs once a new game is started.
        if (image == null)
        {
            Debug.LogWarning("Warning: Attempted to start game with no image loaded.");
            return;
        }

        currentState = State.Active;
        elapsedTime = 0f;

        Debug.Log($"Game Started, State set to: {currentState}");
    }
    public void PauseGame()
    {
        // Anything that triggers during a paused game.
        if (currentState == State.Active)
        {
            currentState = State.Paused;
        }
    }
    public void ResumeGame()
    {
        // Anything that should trigger after resuming the game.
        if (currentState == State.Paused)
        {
            currentState = State.Active;
        }
    }
    public void RestartGame()
    {
        Debug.Log("Game restarting.");

        currentState = State.Inactive;
        image = null;
        elapsedTime = 0f;
        
        if (timer != null)
        {
            timer.text = "00:00:00"; 
        }

        ClearPuzzle();
        
    }
    public void ResetPuzzle()
    {
        if (image == null) 
        {
            Debug.LogWarning("No image loaded.");
            return;
        }

        GenerateNewPuzzle(image);
        StartGame();

        Debug.Log("Puzzle reset.");
    }
    public void RestartTimer()
    {
        if (image != null)
        {
            elapsedTime = 0f;
            currentState = State.Active;
            Debug.Log("Timer reset.");
        }
    }
    public void Update()
    {
        // Anything that updates as the game plays should be put into here.
        if (currentState == State.Active)
        {
            // Anything requiring an active game should be put in here.
            elapsedTime += Time.deltaTime;

            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            int milliseconds = Mathf.FloorToInt(elapsedTime * 1000 % 1000);

            if (timer != null)
            {
                timer.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
            }
        }
    }

    public void GenerateNewPuzzle(Texture loadedImage)
    {
        ClearPuzzle();

        image = loadedImage;

        Vector2 puzzleSize = GetBoardSize(image, boardWidth, boardHeight);
        
        pieceMaterial.mainTexture = loadedImage;

        int generationSeed = System.Guid.NewGuid().GetHashCode();

        GameModeSettings modeSettings = GetCurrentGameModeSettings();

        int rows = modeSettings.rows;
        int columns = modeSettings.columns;

        float pieceWidth = puzzleSize.x / columns;
        float pieceHeight = puzzleSize.y / rows;
        float smallestSide = Mathf.Min(pieceWidth, pieceHeight);

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

        //Hard code values for testing purposes
        PuzzleConfig puzzleConfig = new PuzzleConfig
        {
            rows = rows,
            columns = columns,
            generationSeed = generationSeed,
            puzzleImage = loadedImage,
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

        var groupSystem = new GroupSystem();
        var connectionSystem = new ConnectionSystem(groupSystem);

        groupSystem.Initialize(piecesData);

        if (snappingManager != null)
        {
            snappingManager.connectionSystem = connectionSystem;
        }
        else
        {
            Debug.LogWarning("SnappingManager not assigned.");
        }
    }

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

    [SerializeField] private float boardWidth = 8f;
    [SerializeField] private float boardHeight = 6f;
    private Vector2 GetBoardSize(Texture image, float maxWidth, float maxHeight)
    {
        float imageAspect = image.width / (float)image.height;
        float boxAspect = maxWidth / maxHeight;

        if (imageAspect > boxAspect)
        {
            return new Vector2(maxWidth, maxWidth / imageAspect);
        }

        return new Vector2(maxHeight * imageAspect, maxHeight);
    }

    private GameModeSettings GetCurrentGameModeSettings()
    {
        switch (currentGameMode)
        {
            case GameMode.Easy:
                return easyMode;

            case GameMode.Medium:
                return mediumMode;

            case GameMode.Hard:
                return hardMode;

            default:
                return easyMode;
        }
    }

    public void SelectEasyMode()
    {
        currentGameMode = GameMode.Easy;

        Debug.Log("Selected Easy Mode");
    }
    public void SelectMediumMode()
    {
        currentGameMode = GameMode.Medium;

        Debug.Log("Selected Medium Mode");
    }

    public void SelectHardMode()
    {
        currentGameMode = GameMode.Hard;

        Debug.Log("Selected Hard Mode");
    }
}

