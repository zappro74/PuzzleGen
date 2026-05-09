using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class SnappingManager : MonoBehaviour
{
    [Header("Snapping Settings")]
    public float snappingTolerance = 0.5f;

    [Header("Animation Settings")]
    public float snapSpeed = 0.25f; // the speed value of snapping
    public AnimationCurve snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // the curve inside the inspector that controls the snapping speed

    [Header("Effects")]
    public ParticleSystem snapParticles;

    public ConnectionSystem connectionSystem;
    private SnapValidation snapValidator = new SnapValidation();

    [Header("Audio")]
    [SerializeField] private AudioSource snapAudioSource;

    [SerializeField] private AudioClip[] snapSounds;

    [Header("Game")]
    [SerializeField] private GameStateManager gameStateManager;

    public void TrySnap(Transform pieceGroup)
    {
        if (connectionSystem == null)
        {
            Debug.LogWarning("Missing connection system.");
            return;
        }
        var groupedPieces = pieceGroup.GetComponentsInChildren<PuzzlePiece>();
        var allPieces = FindObjectsByType<PuzzlePiece>(FindObjectsInactive.Exclude);

        foreach (var groupPiece in groupedPieces)
        {
            foreach (var piece in allPieces)
            {
                if (groupPiece == piece || groupPiece.Data.GroupId == piece.Data.GroupId)
                {
                    continue;
                }

                if (snapValidator.CanSnap(groupPiece.Data, piece.Data, groupPiece.transform, piece.transform, snappingTolerance))
                {    
    
                    Vector2 solvedOffset = (Vector2)groupPiece.SolvedPosition - (Vector2)piece.SolvedPosition;
                    Quaternion rotation = GetRoot(piece.transform).rotation;
                    Vector2 rotatedOffset = rotation * solvedOffset;
                    Vector2 snappingPosition = (Vector2)piece.transform.position + rotatedOffset;
                    var distance = Vector2.Distance(groupPiece.transform.position, snappingPosition);

                    if (distance <= snappingTolerance)
                    {
                        Vector3 adjustment = (Vector3)snappingPosition - groupPiece.transform.position;
                        Transform targetRoot = GetRoot(piece.transform);
                        connectionSystem.AddConnection(groupPiece.Data, piece.Data);

                        Vector3 startWorldPosition = pieceGroup.position;
                        pieceGroup.position += adjustment;

                        Vector3 endWorldPosition = pieceGroup.position;
                        pieceGroup.position = startWorldPosition;

                        StartCoroutine(Animate(pieceGroup, targetRoot, startWorldPosition, endWorldPosition, groupPiece, piece));

                        Debug.Log($"Snapped Piece {groupPiece.Data.Id} to Piece {piece.Data.Id}");

                        return;
                    }
                }
            }
        }
    }

    public bool TryAutoSnap(Transform pieceGroup)
    {
        if (connectionSystem == null)
        {
            Debug.LogWarning("Missing connection system.");
            return false;
        }

        PuzzlePiece[] groupedPieces = pieceGroup.GetComponentsInChildren<PuzzlePiece>();
        PuzzlePiece[] allPieces = FindObjectsByType<PuzzlePiece>(FindObjectsInactive.Exclude);

        foreach (PuzzlePiece groupPiece in groupedPieces)
        {
            foreach (PuzzlePiece piece in allPieces)
            {
                if (groupPiece == piece || groupPiece.Data.GroupId == piece.Data.GroupId)
                {
                    continue;
                }

                if (snapValidator.CanSnap(groupPiece.Data, piece.Data, groupPiece.transform, piece.transform, snappingTolerance))
                {
                    Vector2 solvedOffset = (Vector2)groupPiece.SolvedPosition - (Vector2)piece.SolvedPosition;
                    Quaternion rotation = GetRoot(piece.transform).rotation;
                    Vector2 rotatedOffset = rotation * solvedOffset;
                    Vector2 snappingPosition = (Vector2)piece.transform.position + rotatedOffset;
                    float distance = Vector2.Distance(groupPiece.transform.position, snappingPosition);

                    if (distance <= snappingTolerance)
                    {
                        if (!IsValidSnapRotation(pieceGroup, piece.transform))
                        {
                            continue;
                        }

                        Vector3 adjustment = (Vector3)snappingPosition - groupPiece.transform.position;
                        Transform targetRoot = GetRoot(piece.transform);
                        connectionSystem.AddConnection(groupPiece.Data, piece.Data);

                        Vector3 startWorldPosition = pieceGroup.position;
                        pieceGroup.position += adjustment;

                        Vector3 endWorldPosition = pieceGroup.position;
                        pieceGroup.position = startWorldPosition;

                        StartCoroutine(Animate(pieceGroup, targetRoot, startWorldPosition, endWorldPosition, groupPiece, piece));

                        Debug.Log($"Auto snapped Piece {groupPiece.Data.Id} to Piece {piece.Data.Id}");

                        return true;
                    }
                }
            }
        }
        return false;
    }
    private IEnumerator Animate(Transform pieceGroup, Transform targetRoot, Vector3 start, Vector3 end, PuzzlePiece groupPiece, PuzzlePiece piece)
    {
        float elapsedTime = 0f;

        Collider2D[] colliders = pieceGroup.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        while (elapsedTime < snapSpeed)
        {
            elapsedTime += Time.deltaTime;
            pieceGroup.position = Vector3.Lerp(start, end, snapCurve.Evaluate(elapsedTime / snapSpeed));
            yield return null;
        }

        pieceGroup.position = end;

        MergeGroups(pieceGroup, targetRoot);

        PlaySnapSound();
        Particles(groupPiece, piece);
        RestorePieceRenderers(targetRoot);

        foreach (Collider2D collider in colliders)
        {
            collider.enabled = true;
        }

        if (connectionSystem.Groups.IsPuzzleComplete())
        {
            gameStateManager.WinGame();
        }
    }
    private void Particles(PuzzlePiece groupPiece, PuzzlePiece piece)
    {          
        if (snapParticles == null || !groupPiece.TryGetComponent(out Renderer gRender) || !piece.TryGetComponent(out Renderer tRender))
        {
            return;
        }

        var spreadFrom = 0.9f;

        var direction = groupPiece.SolvedPosition - piece.SolvedPosition;
        var seam = new Vector3(-direction.y, direction.x, 0f).normalized;
        
        Vector3 center = ((Vector2)gRender.bounds.center + (Vector2)tRender.bounds.center) / 2f;

        Vector3[] directions = { seam, -seam };
        
        foreach (var d in directions)
        {
            var particleSys = Instantiate(snapParticles, center + (d * spreadFrom), Quaternion.LookRotation(d));
            if (particleSys.TryGetComponent(out ParticleSystemRenderer renderer)) 
            {
                renderer.sortingOrder = Mathf.Max(gRender.sortingOrder, tRender.sortingOrder) + 10;
            }
            Destroy(particleSys.gameObject, 1f);
        }            
                                
    }
    private Transform GetRoot(Transform piece)
    {
        while (piece.parent != null && piece.parent.GetComponent<PuzzlePiece>() != null)
        {
            piece = piece.parent;
        }

        return piece;
    }

    private void RestorePieceRenderers(Transform groupRoot)
    {
        PuzzlePiece[] pieces = groupRoot.GetComponentsInChildren<PuzzlePiece>();

        foreach (PuzzlePiece puzzlePiece in pieces)
        {
            MeshRenderer meshRenderer = puzzlePiece.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
            }
        }

        Transform combinedVisual = groupRoot.Find("CombinedVisual");

        if (combinedVisual != null)
        {
            combinedVisual.gameObject.SetActive(false);
        }
    }

    private void PlaySnapSound()
    {
        if (snapAudioSource == null || snapSounds.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, snapSounds.Length);

        snapAudioSource.pitch = Random.Range(0.95f, 1.05f);

        snapAudioSource.PlayOneShot(snapSounds[randomIndex]);
    }

    private bool IsValidSnapRotation(Transform pieceGroup, Transform targetPiece)
    {
        float angleA = Mathf.Round(pieceGroup.eulerAngles.z);
        float angleB = Mathf.Round(targetPiece.eulerAngles.z);

        float difference = Mathf.Abs(Mathf.DeltaAngle(angleA, angleB));

        return difference <= 2f;
    }

    private void MergeGroups(Transform sourceRoot, Transform targetRoot)
    {
        if (sourceRoot == targetRoot)
        {
            return;
        }

        List<Transform> children = new List<Transform>();

        foreach (Transform child in sourceRoot)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            child.SetParent(targetRoot, true);
        }

        sourceRoot.SetParent(targetRoot, true);
    }
}
