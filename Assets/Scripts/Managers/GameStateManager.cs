using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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
        currentState = State.Inactive;
        image = null;
        elapsedTime = 0f;
        
        if (timer != null)
        {
            timer.text = "Time: 00:00"; 
        }
        
        Debug.Log("Game reset called.");
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
        image = loadedImage;

        int generationSeed = System.Guid.NewGuid().GetHashCode();

        //might need this to be scalable... 
        PieceConfig pieceConfig = new PieceConfig
        {
            pieceMaterial = pieceMaterial,
            tabHeight = 0.2f,
            edgeMargin = 0.25f,
            tabWidth = 0.4f,
            pointsPerCurveHalf = 8
        };

        //Hard code values for testing purposes
        PuzzleConfig puzzleConfig = new PuzzleConfig
        {
            rows = 3,
            columns = 3,
            generationSeed = generationSeed,
            pieceConfig = pieceConfig
        };

        List<GameObject> pieces = puzzleFactory.GeneratePuzzle(puzzleConfig, image.width, image.height);
    }
}

