using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;

public class PieceFactory : MonoBehaviour
{


    public GameObject CreatePiece(PieceData pieceData, Material pieceMaterial, float pieceWidth, float pieceHeight, float tabHeight, float edgeMargin, float tabWidth, int pointsPerCurveHalf)
    {
        List<Vector2> outlinePoints = PieceOutline.BuildPieceOutline(pieceData, pieceWidth, pieceHeight, tabHeight, edgeMargin, tabWidth, pointsPerCurveHalf);

        GameObject pieceObject = new GameObject($"Piece_{pieceData.Id}");

        MeshFilter meshFilter = pieceObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = pieceObject.AddComponent<MeshRenderer>();
        PolygonCollider2D polygonCollider = pieceObject.AddComponent<PolygonCollider2D>();
        PuzzlePiece puzzlePiece = pieceObject.AddComponent<PuzzlePiece>();

        /*
        Mesh pieceMesh = BuildMesh(outlinePoints); //creating a helper for this shortly

        meshFilter.mesh = pieceMesh;
        */
        
        meshRenderer.material = pieceMaterial;

        polygonCollider.points = outlinePoints.ToArray();

        puzzlePiece.Initialize(pieceData);

        return pieceObject;
    }

    /*
    private Mesh BuildMesh(List<Vector2> outlinePoints)
    {
        Mesh mesh = new Mesh();

        Vector3[] verticies = new Vector3[outlinePoints.Count];

        for (int i = 0; i < outlinePoints.Count; i++)
        {
            verticies[i] = new Vector3(outlinePoints[i].x, outlinePoints[i].y, 0f);
        }

        //Need triangulation...
        //Fun Research time!!!
    }
    */
}
