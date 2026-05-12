using UnityEngine;
using TMPro;

public enum State { Inactive, Active, Paused }
public class GameStateManager : MonoBehaviour
{
    public State currentState;

    [Header("Puzzle Image")]
    public Texture image; 

    [Header("UI Connections")]
    public TextMeshProUGUI timer;
    public GameObject modeSelectionPanel;

    [Header("Script Connections")]
    public WinManager winManager;
    public GameModeController modeController;
    public PuzzleBuilder puzzleBuilder;


    public float elapsedTime = 0f;

    public void StartGame()
    {
        winManager.ResetWinScreen();

        if (timer != null)
        {
            timer.gameObject.SetActive(true);
        }
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
        winManager.ResetWinScreen();
        image = loadedImage;
        puzzleBuilder.ClearPuzzle();
        
        if (modeSelectionPanel != null)
        {
            modeSelectionPanel.SetActive(true);
        }
    }

    public void PauseGame()
    {
        if (currentState == State.Active)
        {
            currentState = State.Paused;
        }
    }
    public void ResumeGame()
    {
        if (currentState == State.Paused)
        {
            currentState = State.Active;
        }
    }
    public void RestartGame()
    {
        Debug.Log("Game restarting.");

        currentState = State.Inactive;
        elapsedTime = 0f;
        
        if (timer != null)
        {
            timer.text = "00:00:00"; 
        }

        ResetPuzzle();
    }
    public void ResetPuzzle()
    {
        if (image == null)
        {
            Debug.LogWarning("No image loaded.");
            return;
        }

        if (winManager.hasWon)
        {
            Debug.Log("Resetting Win Screen");
            winManager.ResetWinScreen();
        }

        puzzleBuilder.ClearPuzzle();
        Debug.Log("Puzzle cleared.");

        if (modeSelectionPanel != null)
        {
            modeSelectionPanel.SetActive(true);
        }
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
        if (currentState == State.Active)
        {
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
}
