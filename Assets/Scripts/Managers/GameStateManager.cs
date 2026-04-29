using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    public enum State
    {
        Active,
        Inactive
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
        currentState = State.Active;
        elapsedTime = 0;

        Debug.Log($"Game Started, State set to: {currentState}");
    }
    public void Update()
    {
        // Anything that updates as the game plays should be put into here.
        if (currentState == State.Active)
        {
            // Anything requiring an active game should be put in here.
            elapsedTime += Time.deltaTime;
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

