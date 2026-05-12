using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinManager : MonoBehaviour
{
    [Header("Script Connections")]
    public AudioManager audioManager;
    public GameModeController modeController;
    public GameStateManager gameStateManager;
    public PuzzleBuilder puzzleBuilder;
    public VisualFunctions visualFunctions;
    
    [Header("Win Screen")]
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private ParticleSystem[] confettiCannons;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private RawImage solvedImageDisplay;
    [SerializeField] private float spinSpeed = 50f;
    [SerializeField] private float bounceSpeedX = 200f;
    [SerializeField] private float bounceSpeedY = 200f;
    
    public bool hasWon = false;
    private Vector2 bounceDirection = new Vector2(1f, 1f);

    public void Update()
    {
        if (hasWon && solvedImageDisplay != null)
        {
            solvedImageDisplay.rectTransform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            RectTransform rect = solvedImageDisplay.rectTransform;

            Vector2 movement = new Vector2(bounceSpeedX, bounceSpeedY);

            rect.anchoredPosition += bounceDirection * movement * Time.deltaTime;

            Canvas canvas = solvedImageDisplay.canvas;

            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();

                float halfWidth = rect.rect.width * rect.localScale.x * 0.5f;
                float halfHeight = rect.rect.height * rect.localScale.y * 0.5f;

                float leftBound = -canvasRect.rect.width * 0.5f + halfWidth;
                float rightBound = canvasRect.rect.width * 0.5f - halfWidth;

                float bottomBound = -canvasRect.rect.height * 0.5f + halfHeight;
                float topBound = canvasRect.rect.height * 0.5f - halfHeight;

                Vector2 pos = rect.anchoredPosition;

                if (pos.x < leftBound || pos.x > rightBound)
                {
                    bounceDirection.x *= -1f;
                }

                if (pos.y < bottomBound || pos.y > topBound)
                {
                    bounceDirection.y *= -1f;
                }
    
            }

            if (audioManager != null)
            {
                float bass = audioManager.GetCurrentBass();
                float scale = Mathf.Clamp(1f + bass, 1f, 4f);
                solvedImageDisplay.rectTransform.localScale = Vector3.Lerp(solvedImageDisplay.rectTransform.localScale, new Vector3(scale, scale, scale), Time.deltaTime * 10f);
            }
        }
    }

    public void ResetWinScreen()
    {
        hasWon = false;
        winScreenPanel?.SetActive(false);
        if (solvedImageDisplay != null)
        {
            solvedImageDisplay.texture = null;
            solvedImageDisplay.gameObject.SetActive(false);
        }
        
        if (audioManager != null) audioManager.StopWinAudio();
        if (visualFunctions != null) visualFunctions.StopConfetti();     
    }

    public void WinGame()
    {
        var elapsedTime = gameStateManager.elapsedTime;
        var image = gameStateManager.image;
        
        if (hasWon)
        {
            return;
        }

        hasWon = true;

        Leaderboard.SubmitTime(modeController.GetCurrentGameModeSettings().mode, elapsedTime);

        gameStateManager.currentState = State.Paused;

        if (gameStateManager.timer != null)
        {
            gameStateManager.timer.gameObject.SetActive(false);
        }

        puzzleBuilder.ClearPuzzle();

        Debug.Log("Puzzle Complete!");

        if (finalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);

            finalTimeText.text = $"You solved the puzzle in: {minutes} minute(s) {seconds} seconds!";
        }

        if (audioManager != null) StartCoroutine(audioManager.PlayWinAudio());

        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
        }

        if (solvedImageDisplay != null)
        {
            solvedImageDisplay.gameObject.SetActive(true);
            solvedImageDisplay.texture = image;
            solvedImageDisplay.color = Color.white;

            solvedImageDisplay.rectTransform.anchoredPosition = Vector2.zero;
            solvedImageDisplay.rectTransform.localScale = Vector3.one;
        }

        if (visualFunctions != null) StartCoroutine(visualFunctions.RepeatingConfetti());

        if (!string.IsNullOrEmpty(JSONFunctions.JSONFileFunctions.CurrentSaveFilePath))
        {
            if (File.Exists(JSONFunctions.JSONFileFunctions.CurrentSaveFilePath))
            {
                File.Delete(JSONFunctions.JSONFileFunctions.CurrentSaveFilePath);

                Debug.Log("Deleted completed puzzle save file.");
            }
        }
    }
}
