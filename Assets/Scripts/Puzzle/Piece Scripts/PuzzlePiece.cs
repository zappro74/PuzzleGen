using System.Collections;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    public PieceData Data { get; private set; }
    public Vector3 SolvedPosition { get; set; }

    public void Initialize(PieceData pieceData)
    {
        Data = pieceData;
        SolvedPosition = transform.position;
    }
    public void UpdatePosition()
    {
        SolvedPosition = transform.position;
    }

    public IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
    Vector3 start = transform.position;

    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;

        float t = elapsed / duration;
        t = Mathf.SmoothStep(0f, 1f, t);

        transform.position = Vector3.Lerp(start, targetPosition, t);

        yield return null;
    }

    transform.position = targetPosition;
    }
}
