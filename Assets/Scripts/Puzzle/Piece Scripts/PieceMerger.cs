using System.Collections.Generic;
using UnityEngine;

public class PieceMerger : MonoBehaviour
{
    public void MergeGroups(Transform sourceRoot, Transform targetRoot)
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

        if (targetRoot.GetComponent<PuzzlePiece>() != null)
        {
            foreach (var piece in targetRoot.GetComponentsInChildren<PuzzlePiece>())
            {
                if (piece.transform == targetRoot)
                {
                    continue;
                }

                Vector3 position = (Vector2)piece.SolvedPosition - (Vector2)targetRoot.GetComponent<PuzzlePiece>().SolvedPosition;
                position.z = piece.transform.localPosition.z;

                piece.transform.localPosition = position;
                piece.transform.localRotation = Quaternion.identity;
            }
        }
    }
    public Transform GetRoot(Transform piece)
    {
        while (piece.parent != null && piece.parent.CompareTag("Piece"))
        {
            piece = piece.parent;
        }

        return piece;
    }
}
