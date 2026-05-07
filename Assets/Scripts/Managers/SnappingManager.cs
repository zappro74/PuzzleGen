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

    private float snapSearchRadius = 1.5f;

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

                if (snapValidator.CanSnap(groupPiece.Data, piece.Data, snappingTolerance))
                {    
    
                    var snappingPosition = (Vector2)piece.transform.position + ((Vector2)groupPiece.SolvedPosition - (Vector2)piece.SolvedPosition);
                    var distance = Vector2.Distance(groupPiece.transform.position, snappingPosition);

                    if (distance <= snappingTolerance)
                    {
                        var adjustment = (Vector3)snappingPosition - groupPiece.transform.position;

                        pieceGroup.SetParent(GetRoot(piece.transform));
                        connectionSystem.AddConnection(groupPiece.Data, piece.Data);

                        var startLocalPosition = pieceGroup.localPosition;
                        pieceGroup.position += adjustment;
                        var endLocalPosition = pieceGroup.localPosition;

                        pieceGroup.localPosition = startLocalPosition;

                        StartCoroutine(Animate(pieceGroup, startLocalPosition, endLocalPosition, groupPiece, piece));

                        Debug.Log($"Snapped Piece {groupPiece.Data.Id} to Piece {piece.Data.Id}");
                        return;
                    }
                }
            }
        }
    }

    private IEnumerator Animate(Transform pieceGroup, Vector3 start, Vector3 end, PuzzlePiece groupPiece, PuzzlePiece piece)
    {
        var elapsedTime = 0f;
        var colliders = pieceGroup.GetComponentsInChildren<Collider2D>();

        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        while (elapsedTime < snapSpeed)
        {
            elapsedTime += Time.deltaTime;
            pieceGroup.localPosition = Vector3.Lerp(start, end, snapCurve.Evaluate(elapsedTime / snapSpeed));
            yield return null; 
        }

        pieceGroup.localPosition = end;

        PlaySnapSound();

        Particles(groupPiece, piece);

        //RebuildMesh(GetRoot(piece));
        RestorePieceRenderers(GetRoot(pieceGroup));

        foreach (var collider in colliders)
        {
            collider.enabled = true;
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

    private void RebuildMesh(Transform groupRoot)
    {
        PuzzlePiece[] pieces = groupRoot.GetComponentsInChildren<PuzzlePiece>();

        if (pieces.Length == 0)
        {
            return;
        }

        List<CombineInstance> combines = new List<CombineInstance>();

        foreach (PuzzlePiece puzzlePiece in pieces)
        {
            MeshFilter meshFilter = puzzlePiece.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = puzzlePiece.GetComponent<MeshRenderer>();

            if ((meshFilter == null) || (meshRenderer == null))
            {
                continue;
            }

            combines.Add(new CombineInstance
            {
                mesh = meshFilter.sharedMesh,
                transform = groupRoot.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix
            });

            meshRenderer.enabled = false;
        }

        Transform combinedVisual = groupRoot.Find("CombinedVisual");

        if (combinedVisual == null)
        {
            GameObject visualObject = new GameObject("CombinedVisual");
            visualObject.transform.SetParent(groupRoot);
            visualObject.transform.localPosition = Vector3.zero;
            visualObject.transform.localRotation = Quaternion.identity;
            visualObject.transform.localScale = Vector3.one;

            combinedVisual = visualObject.transform;
        }

        MeshFilter groupMeshFilter = combinedVisual.GetComponent<MeshFilter>();
        MeshRenderer groupMeshRenderer = combinedVisual.GetComponent<MeshRenderer>();

        if (groupMeshFilter == null)
        {
            groupMeshFilter = combinedVisual.gameObject.AddComponent<MeshFilter>();
        }

        if (groupMeshRenderer == null)
        {
            groupMeshRenderer = combinedVisual.gameObject.AddComponent<MeshRenderer>();
        }

        Mesh combinedMesh = new Mesh();

        combinedMesh.CombineMeshes(combines.ToArray(), true, true);

        combinedMesh.RecalculateBounds();
        combinedMesh.RecalculateNormals();

        combinedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);

        groupMeshFilter.mesh = combinedMesh;
        groupMeshRenderer.sharedMaterial = pieces[0].GetComponent<MeshRenderer>().sharedMaterial;
        groupMeshRenderer.sortingOrder = 10;
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
}
