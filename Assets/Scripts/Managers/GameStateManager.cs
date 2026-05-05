using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.U2D;

public class GameStateManager : MonoBehaviour
{
    public enum State
    {
        Inactive,
        Active,
        Paused
    }
    public State currentState;

    [Header("Puzzle Image")]
    public Texture2D image; 

    [Header("UI Connections")]
    public TextMeshProUGUI timer;

    [Header("Script Connections")]
    public SnappingManager snappingManager;

    private float elapsedTime = 0f;

    [SerializeField] private PuzzleFactory puzzleFactory;
    [SerializeField] private Material pieceMaterial;
    public void StartGame()
    {
        // Runs once a new game is started.
        if (image == null)
        {
            Debug.LogWarning("Warning: Attempted to start game with no image loaded into unity.");
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
            timer.text = "Time: 00:00"; 
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

            if (timer != null)
            {
                timer.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    public void GenerateNewPuzzle(Texture2D loadedImage)
    {
        ClearPuzzle();

        image = loadedImage;

        Vector2 puzzleSize = GetBoardSize(image, boardWidth, boardHeight);
        
        pieceMaterial.mainTexture = loadedImage;

        int generationSeed = System.Guid.NewGuid().GetHashCode();

        //might need this to be scalable... 
        PieceConfig pieceConfig = new PieceConfig
        {
            pieceMaterial = pieceMaterial,
            pieceWidth = puzzleSize.x / 3,
            pieceHeight = puzzleSize.y / 3,
            tabHeight = 0.5f,
            edgeMargin = 0.35f,
            tabWidth = 0.35f,
            pointsPerCurveHalf = 100
        };

        //Hard code values for testing purposes
        PuzzleConfig puzzleConfig = new PuzzleConfig
        {
            rows = 3,
            columns = 3,
            generationSeed = generationSeed,
            puzzleImage = loadedImage,
            pieceConfig = pieceConfig
        };

        List<GameObject> pieces = puzzleFactory.GeneratePuzzle(puzzleConfig, puzzleSize.x, puzzleSize.y);
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
    private Vector2 GetBoardSize(Texture2D image, float maxWidth, float maxHeight)
    {
        float imageAspect = image.width / (float)image.height;
        float boxAspect = maxWidth / maxHeight;

        if (imageAspect > boxAspect)
        {
            return new Vector2(maxWidth, maxWidth / imageAspect);
        }

        return new Vector2(maxHeight * imageAspect, maxHeight);
    }
}

