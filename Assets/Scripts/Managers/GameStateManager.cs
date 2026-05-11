using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
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

    [Header("Camera Zoom")]
    [SerializeField] private Camera gameCamera;

    [Header("Puzzle Image")]
    public Texture image; 

    [Header("UI Connections")]
    public TextMeshProUGUI timer;
    public GameObject modeSelectionPanel;

    [Header("Script Connections")]
    public SnappingManager snappingManager;
    public GameModeController modeController;
    public InteractionManager interactionManager;


    [Header("Shuffling")]
    [SerializeField] private ExplosionShuffle explosionShuffle;

    private Vector2 bounceDirection = new Vector2(1f, 1f);
    
    private GroupSystem groupSystem;
    private ConnectionSystem connectionSystem;

    public float elapsedTime = 0f;

    public void StartGame()
    {
        ResetWinScreen();

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
        ResetWinScreen();

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

        if (hasWon)
        {
            Debug.Log("Resetting Win Screen");
            ResetWinScreen();
        }

        ClearPuzzle();
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
    private IEnumerator DriftToSavedPosition(Transform piece, Vector3 startPosition, Vector3 targetPosition, float targetRotationZ, float driftDuration)
    {
        Quaternion startingRotation = Quaternion.identity;
        Quaternion targetRotation   = Quaternion.Euler(0f, 0f, targetRotationZ);

        float elapsed = 0f;

        while (elapsed < driftDuration)
        {
            if (piece == null) 
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / driftDuration);
            float progressSmoothed = Mathf.SmoothStep(0f, 1f, progress);

            piece.position = Vector3.Lerp(startPosition, targetPosition, progressSmoothed);
            piece.rotation = Quaternion.Lerp(startingRotation, targetRotation, progressSmoothed);

            yield return null;
        }

        if (piece == null) 
        {
            yield break;
        }

        piece.position = targetPosition;
        piece.rotation = targetRotation;
    }


    private IEnumerator LerpCameraZoom(float startSize, float targetSize, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (gameCamera == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            gameCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

            yield return null;
        }

        if (gameCamera != null)
        {
            gameCamera.orthographicSize = targetSize;
        }
    }
    private IEnumerator LerpCameraPosition(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (gameCamera == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            gameCamera.transform.position = Vector3.Lerp(from, to, t);

            yield return null;
        }

        if (gameCamera != null)
        {
            gameCamera.transform.position = to;
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

            //Chat helped me make this equilizer effect... I really wanted this effect to work!
            if (winMusicSource != null && winMusicSource.isPlaying)
            {
                winMusicSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

                float bass = 0f;

                for (int i = 0; i < 1; i++)
                {
                    bass += spectrum[i];
                }

                bass *= 8f;

                float scale = Mathf.Clamp(1f + bass, 1f, 4f);

                solvedImageDisplay.rectTransform.localScale = Vector3.Lerp(solvedImageDisplay.rectTransform.localScale, new Vector3(scale, scale, scale), Time.deltaTime * 10f);
            }
        }
    }    

}

