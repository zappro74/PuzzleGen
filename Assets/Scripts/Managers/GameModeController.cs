using UnityEngine;

public class GameModeController : MonoBehaviour
{
    [Header("Game Modes")]
    [SerializeField] public GameMode currentGameMode = GameMode.Easy;

    [Header("UI Connections")]
    public GameObject modePanel;

    [Header("Script Connections")]
    public GameStateManager gameManager;

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


    public GameModeSettings GetCurrentGameModeSettings()
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
        modePanel.gameObject.SetActive(false); 
        gameManager.GenerateNewPuzzle(gameManager.image);     
        gameManager.StartGame();
    }

    public void SelectMediumMode()
    {
        currentGameMode = GameMode.Medium;
        modePanel.gameObject.SetActive(false); 
        gameManager.GenerateNewPuzzle(gameManager.image);     
        gameManager.StartGame();
    }

    public void SelectHardMode()
    {
        currentGameMode = GameMode.Hard;
        modePanel.gameObject.SetActive(false); 
        gameManager.GenerateNewPuzzle(gameManager.image);     
        gameManager.StartGame();
    }
}
