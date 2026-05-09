using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.U2D;
using Unity.VisualScripting;
using System.Collections;

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
    public GameObject modeSelectionPanel;

    [Header("Script Connections")]
    public SnappingManager snappingManager;
    public GameModeController modeController;


    [Header("Puzzle Generation")]
    [SerializeField] private PuzzleFactory puzzleFactory;
    [SerializeField] private Material pieceMaterial;

    [Header("Shuffling")]
    [SerializeField] private ExplosionShuffle explosionShuffle;

    [Header("Win Screen")]
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private ParticleSystem[] confettiCannons;
    [SerializeField] private AudioSource winAudioSource;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioSource winMusicSource;
    [SerializeField] private AudioClip winMusic;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private RawImage solvedImageDisplay;
    [SerializeField] private float spinSpeed = 50f;
    

    private bool hasWon = false;

    private GroupSystem groupSystem;
    private ConnectionSystem connectionSystem;

    private float elapsedTime = 0f;

    public void StartGame()
    {
        hasWon = false;

        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(false);
        }

        if (solvedImageDisplay != null)
        {
            solvedImageDisplay.texture = null;
            solvedImageDisplay.gameObject.SetActive(false);
        }

        if (winMusicSource != null)
        {
            winMusicSource.Stop();
        }

        if (timer != null)
        {
            timer.gameObject.SetActive(true);
        }

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
    public void PrepareNewGame(Texture loadedImage)
    {
        hasWon = false;

        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(false);
        }

        if (solvedImageDisplay != null)
        {
            solvedImageDisplay.texture = null;
            solvedImageDisplay.gameObject.SetActive(false);
        }

        hasWon = false;

        image = loadedImage;
        ClearPuzzle();
        
        if (modeSelectionPanel != null)
        {
            modeSelectionPanel.SetActive(true);
        }
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

        hasWon = false;

        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(false);
        }

        if (solvedImageDisplay != null)
        {
            solvedImageDisplay.texture = null;
            solvedImageDisplay.gameObject.SetActive(false);
        }

        if (winMusicSource != null)
        {
            winMusicSource.Stop();
        }

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

        Debug.Log("Puzzle reset.");
        modeSelectionPanel.SetActive(true);    
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
        
        if (hasWon && solvedImageDisplay != null)
        {
            solvedImageDisplay.rectTransform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }
    }

    public void GenerateNewPuzzle(Texture loadedImage)
    {
        ClearPuzzle();

        image = loadedImage;

        Vector2 puzzleSize = GetBoardSize(image, boardWidth, boardHeight);
        
        pieceMaterial.mainTexture = loadedImage;

        int generationSeed = System.Guid.NewGuid().GetHashCode();

        GameModeSettings modeSettings = modeController.GetCurrentGameModeSettings();

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

        groupSystem = new GroupSystem();
        connectionSystem = new ConnectionSystem(groupSystem);

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

    public void WinGame()
    {
        if (hasWon)
        {
            return;
        }

        hasWon = true;

        currentState = State.Paused;

        if (timer != null)
        {
            timer.gameObject.SetActive(false);
        }

        ClearPuzzle();

        Debug.Log("Puzzle Complete!");

        if (finalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);

            finalTimeText.text = $"You solved the puzzle in: {minutes} minute(s) {seconds} seconds!";
        }

        if (winAudioSource != null && winSound != null)
        {
            winAudioSource.PlayOneShot(winSound);
        }

        if (winMusicSource != null && winMusic != null)
        {
            winMusicSource.clip = winMusic;
            winMusicSource.loop = true;
            winMusicSource.Play();
        }

        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
        }

        if (solvedImageDisplay != null)
        {
            solvedImageDisplay.gameObject.SetActive(true);
            solvedImageDisplay.texture = image;
            solvedImageDisplay.color = Color.white;
        }

        foreach (ParticleSystem cannon in confettiCannons)
        {
            if (cannon != null)
            {
                cannon.Play();
            }
        }
    }
}

