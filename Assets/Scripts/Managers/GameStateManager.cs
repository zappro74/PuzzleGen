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


    [Header("Puzzle Generation")]
    [SerializeField] private PuzzleFactory puzzleFactory;
    [SerializeField] private Material pieceMaterial;
    public int currentGenerationSeed;
    public int currentRows;
    public int currentColumns;

    [Header("Shuffling")]
    [SerializeField] private ExplosionShuffle explosionShuffle;
    [Header("Load Sound")]
    [SerializeField] private AudioSource loadAudioSource;
    [SerializeField] private AudioClip loadSound;

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
    [SerializeField] private float bounceSpeedX = 200f;
    [SerializeField] private float bounceSpeedY = 200f;

    private Vector2 bounceDirection = new Vector2(1f, 1f);

    private float[] spectrum = new float[512];
    
    public bool hasWon = false;

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

    public void LoadJSONGame(Texture loadedImage, List<PieceData> savedPieces, int rows, int columns, int generationSeed, float savedElapsedTime = 0f)
    {
        GeneratePuzzleFromJSON(loadedImage, savedPieces, rows, columns, generationSeed, savedElapsedTime);
        currentState = State.Active;
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
    private void ResetWinScreen()
    {
        hasWon = false;
        winScreenPanel?.SetActive(false);
        if (solvedImageDisplay != null)
        {
            solvedImageDisplay.texture = null;
            solvedImageDisplay.gameObject.SetActive(false);
        }
        winMusicSource?.Stop();
        if (confettiCannons != null)
        {
            foreach (ParticleSystem cannon in confettiCannons)
            {
                cannon?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
    public void GeneratePuzzleFromJSON(Texture loadedImage, List<PieceData> savedPieces, int rows, int columns, int generationSeed, float savedElapsedTime = 0f)
    {
        StartCoroutine(
            GeneratePuzzleFromJSONRoutine(
                loadedImage,
                savedPieces,
                rows,
                columns,
                generationSeed,
                savedElapsedTime
            )
        );
    }

    //altered by Claude (I've been trying to get this to work for the past 5 hours) (Zach Procopis)
    //left in Claudes comments for honesty
    private IEnumerator GeneratePuzzleFromJSONRoutine(Texture loadedImage, List<PieceData> savedPieces, int rows, int columns, int generationSeed, float savedElapsedTime = 0f)
    {
        elapsedTime = savedElapsedTime;
        ClearPuzzle();
        yield return null;

        image = loadedImage;
        pieceMaterial.mainTexture = loadedImage;

        currentGenerationSeed = generationSeed;
        currentRows    = rows;
        currentColumns = columns;

        Vector2 puzzleSize = GetBoardSize(image, boardWidth, boardHeight);

        float pieceWidth  = puzzleSize.x / columns;
        float pieceHeight = puzzleSize.y / rows;

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

        PuzzleConfig puzzleConfig = new PuzzleConfig
        {
            rows = rows,
            columns = columns,
            generationSeed = generationSeed,
            puzzleImage = loadedImage,
            pieceConfig = pieceConfig
        };

        List<GameObject> pieces = puzzleFactory.GeneratePuzzle(puzzleConfig, puzzleSize.x, puzzleSize.y);

        Dictionary<int, PieceData> savedById = new Dictionary<int, PieceData>();
        foreach (PieceData saved in savedPieces)
        {
            savedById[saved.Id] = saved;
        }

        // ── Pass 1: place every piece at its SOLVED position (shows full image) ──
        List<PieceData> loadedPiecesData = new List<PieceData>();

        foreach (GameObject piece in pieces)
        {
            piece.tag = "Piece";

            PuzzlePiece script = piece.GetComponent<PuzzlePiece>();
            if (script == null || script.Data == null) continue;

            script.SolvedPosition = piece.transform.position;

            piece.transform.position = script.SolvedPosition;
            piece.transform.rotation = Quaternion.identity;

            script.Data.GroupId = script.Data.Id;
            loadedPiecesData.Add(script.Data);
        }

        // ── Set up systems BEFORE any movement so snapping logic works later ────
        groupSystem = new GroupSystem();
        connectionSystem = new ConnectionSystem(groupSystem);
        groupSystem.Initialize(loadedPiecesData);

        if (snappingManager != null)
            snappingManager.connectionSystem = connectionSystem;

        // Let Unity render one frame so the player sees the assembled puzzle.
        yield return null;
        yield return null;

        float driftDuration = 1.4f;
        float originalZoom  = gameCamera.orthographicSize;
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (PieceData saved in savedPieces)
        {
            minX = Mathf.Min(minX, saved.Position.x);
            maxX = Mathf.Max(maxX, saved.Position.x);
            minY = Mathf.Min(minY, saved.Position.y);
            maxY = Mathf.Max(maxY, saved.Position.y);
        }

        float boundsWidth  = (maxX - minX) + 2f;
        float boundsHeight = (maxY - minY) + 2f;

        Vector3 boundsCenter = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, gameCamera.transform.position.z);

        boundsCenter.x -= 0f; 
        boundsCenter.y -= 0f;

        // Fit orthographic size to the bounds
        float targetZoom = Mathf.Max(boundsHeight / 2f, boundsWidth / 2f / gameCamera.aspect);
        targetZoom = Mathf.Clamp(targetZoom, interactionManager.minZoom, interactionManager.maxZoom);

        // Move camera to center of pieces and zoom out
        StartCoroutine(LerpCameraZoom(originalZoom, targetZoom, 0.5f));
        StartCoroutine(LerpCameraPosition(gameCamera.transform.position, boundsCenter, 0.5f));

        StartCoroutine(LerpCameraZoom(originalZoom, targetZoom, 0.5f));

        PlayLoadSound();
        
        // ── Pass 2: drift all pieces to saved positions ──────────────────────────
        foreach (GameObject piece in pieces)
        {
            PuzzlePiece script = piece.GetComponent<PuzzlePiece>();
            if (script == null || script.Data == null) continue;

            if (!savedById.TryGetValue(script.Data.Id, out PieceData saved)) continue;

            float displacement = Vector3.Distance(piece.transform.position, saved.Position);
            if (displacement < 0.01f) continue;

            StartCoroutine(DriftToSavedPosition(piece.transform, script.SolvedPosition, saved.Position, saved.Rotation, driftDuration));
        }

        yield return new WaitForSeconds(driftDuration);

        // ── Pass 3: restore saved group connections from JSON ────────────────────
        Dictionary<int, List<GameObject>> savedGroups = new Dictionary<int, List<GameObject>>();

        foreach (GameObject piece in pieces)
        {
            PuzzlePiece script = piece.GetComponent<PuzzlePiece>();
            if (script == null || script.Data == null) continue;

            if (!savedById.TryGetValue(script.Data.Id, out PieceData saved)) continue;

            if (!savedGroups.ContainsKey(saved.GroupId))
                savedGroups[saved.GroupId] = new List<GameObject>();

            savedGroups[saved.GroupId].Add(piece);
        }

        float snapZoom = 3f; 

        foreach (var group in savedGroups)
        {
            List<GameObject> groupPieces = group.Value;
            if (groupPieces.Count <= 1) continue;

            foreach (GameObject piece in groupPieces)
            {
                if (piece == null) continue;

                Transform pieceRoot = InteractionManager.GetRoot(piece.transform);

                Vector3 targetCamPos = new Vector3(pieceRoot.position.x, pieceRoot.position.y, gameCamera.transform.position.z);
                StartCoroutine(LerpCameraPosition(gameCamera.transform.position, targetCamPos, 0.1f));
                StartCoroutine(LerpCameraZoom(gameCamera.orthographicSize, snapZoom, 0.1f));

                snappingManager.TryAutoSnap(pieceRoot);

                yield return new WaitForSeconds(0.15f);

                Transform newRoot = InteractionManager.GetRoot(piece.transform);
                if (newRoot != pieceRoot)
                {
                    snappingManager.TryAutoSnap(newRoot);
                    yield return new WaitForSeconds(0.15f);
                }
            }

            yield return new WaitForSeconds(0.1f);

            foreach (GameObject piece in groupPieces)
            {
                if (piece == null) continue;

                Transform pieceRoot = InteractionManager.GetRoot(piece.transform);
                snappingManager.TryAutoSnap(pieceRoot);
                yield return null;
            }
        }

        // Zoom back out to show all pieces when done
        StartCoroutine(LerpCameraZoom(gameCamera.orthographicSize, targetZoom, 0.8f));
        StartCoroutine(LerpCameraPosition(gameCamera.transform.position, boundsCenter, 0.8f));
    }

    private IEnumerator DriftToSavedPosition(Transform piece, Vector3 startPosition, Vector3 targetPosition, float targetRotationZ, float driftDuration)
    {
        Quaternion startingRotation = Quaternion.identity;
        Quaternion targetRotation   = Quaternion.Euler(0f, 0f, targetRotationZ);

        float elapsed = 0f;

        while (elapsed < driftDuration)
        {
            if (piece == null) yield break;

            elapsed += Time.deltaTime;
            float progress         = Mathf.Clamp01(elapsed / driftDuration);
            float progressSmoothed = Mathf.SmoothStep(0f, 1f, progress);

            piece.position = Vector3.Lerp(startPosition, targetPosition, progressSmoothed);
            piece.rotation = Quaternion.Lerp(startingRotation, targetRotation, progressSmoothed);

            yield return null;
        }

        if (piece == null) yield break;

        piece.position = targetPosition;
        piece.rotation = targetRotation;
    }

    private void PlayLoadSound()
    {
        if (loadAudioSource == null || loadSound == null) return;
        StartCoroutine(PlayLoadSoundWithFadeOut());
    }

    private IEnumerator PlayLoadSoundWithFadeOut()
    {
        loadAudioSource.volume = 1f;
        loadAudioSource.PlayOneShot(loadSound);

        yield return new WaitForSeconds(loadSound.length - 1f);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            loadAudioSource.volume = Mathf.Lerp(1f, 0f, elapsed / 0.5f);
            yield return null;
        }

        loadAudioSource.volume = 0f;
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
            gameCamera.orthographicSize = targetSize;
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
            gameCamera.transform.position = to;
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

    public void GenerateNewPuzzle()
    {
        if (image == null) 
        {
            Debug.LogWarning("No image loaded while trying to generate puzzle.");
            return;
        }

        ClearPuzzle();

        Vector2 puzzleSize = GetBoardSize(image, boardWidth, boardHeight);
        
        pieceMaterial.mainTexture = image;

        GameModeSettings modeSettings = modeController.GetCurrentGameModeSettings();

        int rows = modeSettings.rows;
        int columns = modeSettings.columns;

        float pieceWidth = puzzleSize.x / columns;
        float pieceHeight = puzzleSize.y / rows;
        float smallestSide = Mathf.Min(pieceWidth, pieceHeight);

        int generationSeed = System.Guid.NewGuid().GetHashCode();
        
        currentGenerationSeed = generationSeed;
        currentRows = rows;
        currentColumns = columns;

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

        PuzzleConfig puzzleConfig = new PuzzleConfig
        {
            rows = rows,
            columns = columns,
            generationSeed = generationSeed,
            puzzleImage = image,
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
        Leaderboard.SubmitTime(modeController.GetCurrentGameModeSettings().mode, elapsedTime);
        
        if (hasWon)
        {
            return;
        }

        hasWon = true;

        Leaderboard.SubmitTime(modeController.GetCurrentGameModeSettings().mode, elapsedTime);

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

        StartCoroutine(PlayWinAudio());

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

        StartCoroutine(RepeatingConfetti());


        if (!string.IsNullOrEmpty(JSONFunctions.JSONFileFunctions.CurrentSaveFilePath))
        {
            if (File.Exists(JSONFunctions.JSONFileFunctions.CurrentSaveFilePath))
            {
                File.Delete(JSONFunctions.JSONFileFunctions.CurrentSaveFilePath);

                Debug.Log("Deleted completed puzzle save file.");
            }
        }
    }
    private IEnumerator FadeInWinMusic(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            winMusicSource.volume = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        winMusicSource.volume = 1f;
    }

    private IEnumerator RepeatingConfetti()
    {
        while (hasWon)
        {
            confettiCannons[2].Play();
            foreach (ParticleSystem cannon in confettiCannons)
            {
                if (cannon != null)
                {
                    cannon.Play();
                }
            }

            yield return new WaitForSeconds(3f);
        }
    }
    private IEnumerator PlayWinAudio()
    {
        if (winAudioSource != null && winSound != null)
        {
            winAudioSource.PlayOneShot(winSound);
            yield return new WaitForSeconds(winSound.length - 1);
        }

        if (winMusicSource != null && winMusic != null)
        {
            winMusicSource.clip = winMusic;
            winMusicSource.loop = true;
            winMusicSource.volume = 0f;
            winMusicSource.Play();
            StartCoroutine(FadeInWinMusic(4f));
        }
    }
}

