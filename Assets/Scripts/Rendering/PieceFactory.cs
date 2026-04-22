using UnityEngine;
using System.Collections.Generic;
using LibTessDotNet;

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

        Mesh pieceMesh = BuildMesh(outlinePoints);
        meshFilter.mesh = pieceMesh;

        meshRenderer.material = pieceMaterial;

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

        LibTessDotNet.Tess tess = new LibTessDotNet.Tess();

        LibTessDotNet.ContourVertex[] contour = new LibTessDotNet.ContourVertex[outlinePoints.Count];

        for (int i = 0; i < outlinePoints.Count; i++)
        {
            contour[i].Position = new LibTessDotNet.Vec3(outlinePoints[i].x, outlinePoints[i].y, 0f);
        }

        tess.AddContour(contour, LibTessDotNet.ContourOrientation.Original);
        tess.Tessellate(LibTessDotNet.WindingRule.EvenOdd, LibTessDotNet.ElementType.Polygons, 3);

        return tess.Elements;
    }
}
