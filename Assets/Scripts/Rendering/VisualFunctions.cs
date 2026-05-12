using System.Collections;
using UnityEngine;

public class VisualFunctions : MonoBehaviour
{
    [Header("Script Connections")]
    public WinManager winManager;

    [Header("Particle Connections")]
    public ParticleSystem snapParticles;
    public ParticleSystem[] confettiCannons;

    public IEnumerator RepeatingConfetti()
    {
        while (winManager != null && winManager.hasWon)
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
    public void StopConfetti()
{
    if (confettiCannons == null) return;

    foreach (var cannon in confettiCannons)
    {
        if (cannon != null)
        {
            cannon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
    public void SnappingParticles(PuzzlePiece groupPiece, PuzzlePiece piece)
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
    public void RestorePieceRenderers(Transform groupRoot)
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
}
