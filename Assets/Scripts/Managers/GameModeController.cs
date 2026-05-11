using UnityEngine;

public class GameModeController : MonoBehaviour
{
    [Header("Game Modes")]
    public GameMode currentGameMode = GameMode.Easy;

    [Header("UI Connections")]
    public GameObject modePanel;

    [Header("Script Connections")]
    public GameStateManager gameManager;

    private GameModeSettings easyMode = new GameModeSettings
    {
        mode = GameMode.Easy,
        modeName = "Easy",
        rows = 3,
        columns = 3,
        allowRotation = false,
        allowShuffle = false,
        allowPieceCollision = false,
        snapTolerance = 1f,
        explosionForce = 10f,
        randomForce = 5f,
        torqueForce = 5f,
        shuffleDuration = 5f,
        showFullImagePreview = true,
        allowGroupBreaking = false,
        randomizeInitialRotation = false
    };

    private GameModeSettings mediumMode = new GameModeSettings
    {
        mode = GameMode.Medium,
        modeName = "Medium",
        rows = 6,
        columns = 6,
        allowRotation = false,
        allowShuffle = true,
        allowPieceCollision = false,
        snapTolerance = 0.5f,
        explosionForce = 15f,
        randomForce = 5f,
        torqueForce = 5f,
        shuffleDuration = 5f,
        showFullImagePreview = true,
        allowGroupBreaking = false,
        randomizeInitialRotation = false
    };

    private GameModeSettings hardMode = new GameModeSettings
    {
        mode = GameMode.Hard,
        modeName = "Hard",
        rows = 10,
        columns = 10,
        allowRotation = true,
        allowShuffle = true,
        allowPieceCollision = false,
        snapTolerance = 0.4f,
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
