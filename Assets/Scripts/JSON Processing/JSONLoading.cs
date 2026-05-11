using UnityEngine;

public class JSONLoading : MonoBehaviour
{
    public void LoadJSONGame(Texture loadedImage, List<PieceData> savedPieces, int rows, int columns, int generationSeed, float savedElapsedTime = 0f)
    {
        GeneratePuzzleFromJSON(loadedImage, savedPieces, rows, columns, generationSeed, savedElapsedTime);
        currentState = State.Active;
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

        float originalSnapSpeed = snappingManager.snapSpeed;
        snappingManager.snapSpeed = 0.15f; 

        foreach (var group in savedGroups)
        {
            List<GameObject> groupPieces = group.Value;
            if (groupPieces.Count <= 1) continue;

            foreach (GameObject piece in groupPieces)
            {
                if (piece == null) continue;

                Transform pieceRoot = InteractionManager.GetRoot(piece.transform);

                StartCoroutine(LerpCameraPosition(gameCamera.transform.position, new Vector3(pieceRoot.position.x, pieceRoot.position.y, gameCamera.transform.position.z), 0.05f));
                StartCoroutine(LerpCameraZoom(gameCamera.orthographicSize, snapZoom, 0.05f));

                snappingManager.TryAutoSnap(pieceRoot);
                while (snappingManager.IsAnimating) 
                {
                    yield return null;
                }

                Transform newRoot = InteractionManager.GetRoot(piece.transform);
                if (newRoot != pieceRoot)
                {
                    snappingManager.TryAutoSnap(newRoot);
                    while (snappingManager.IsAnimating) 
                    {
                        yield return null;
                    }
                }
            }

            foreach (GameObject piece in groupPieces)
            {
                if (piece == null) continue;

                Transform pieceRoot = InteractionManager.GetRoot(piece.transform);
                snappingManager.TryAutoSnap(pieceRoot);
                while (snappingManager.IsAnimating)
                {
                    yield return null;
                }
            }
        }

        // Restore original snap speed when done
        snappingManager.snapSpeed = originalSnapSpeed;

        // Zoom back out to show all pieces when done
        StartCoroutine(LerpCameraZoom(gameCamera.orthographicSize, targetZoom, 0.8f));
        StartCoroutine(LerpCameraPosition(gameCamera.transform.position, boundsCenter, 0.8f));
    }

}
