using UnityEngine;
using System.Collections.Generic;
using LibTessDotNet;

public class PieceFactory : MonoBehaviour
{
    public GameObject CreatePiece(PieceData pieceData, PuzzleConfig puzzleConfig)
    {
        PieceConfig pieceConfig = puzzleConfig.pieceConfig;

        List<Vector2> outlinePoints = PieceOutline.BuildPieceOutline(pieceData, pieceConfig.pieceWidth, pieceConfig.pieceHeight, pieceConfig);

        GameObject pieceObject = new GameObject($"Piece_{pieceData.Id}");

        MeshFilter meshFilter = pieceObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = pieceObject.AddComponent<MeshRenderer>();
        PolygonCollider2D polygonCollider = pieceObject.AddComponent<PolygonCollider2D>();
        PuzzlePiece puzzlePiece = pieceObject.AddComponent<PuzzlePiece>();

        Mesh pieceMesh = BuildMesh(outlinePoints);
        meshFilter.mesh = pieceMesh;

        meshRenderer.material = pieceConfig.pieceMaterial;

        polygonCollider.points = outlinePoints.ToArray();

        puzzlePiece.Initialize(pieceData);

        return pieceObject;
    }

    private Mesh BuildMesh(List<Vector2> outlinePoints)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[outlinePoints.Count];

        for (int i = 0; i < outlinePoints.Count; i++)
        {
            vertices[i] = new Vector3(outlinePoints[i].x, outlinePoints[i].y, 0f);
        }

        mesh.vertices = vertices;
        mesh.triangles = Triangulate(outlinePoints);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private int[] Triangulate(List<Vector2> outlinePoints)
    {
        //LibTessDotNet is a library/plugin
        //Here's the github: https://github.com/speps/LibTessDotNet/releases

        Tess tess = new Tess();

        ContourVertex[] contour = new ContourVertex[outlinePoints.Count];

        for (int i = 0; i < outlinePoints.Count; i++)
        {
            contour[i].Position = new Vec3(outlinePoints[i].x, outlinePoints[i].y, 0f);
        }

        tess.AddContour(contour, ContourOrientation.Original);
        tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

        return tess.Elements;
    }
}
