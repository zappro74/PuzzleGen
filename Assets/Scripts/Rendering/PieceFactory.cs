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

        Mesh pieceMesh = BuildMesh(outlinePoints, pieceData, puzzleConfig);
        meshFilter.mesh = pieceMesh;

        meshRenderer.material = pieceConfig.pieceMaterial;

        polygonCollider.points = outlinePoints.ToArray();

        puzzlePiece.Initialize(pieceData);

        return pieceObject;
    }

    private Mesh BuildMesh(List<Vector2> outlinePoints, PieceData pieceData, PuzzleConfig puzzleConfig)
    {
        Mesh mesh = new Mesh();

        PieceConfig pieceConfig = puzzleConfig.pieceConfig;

        Vector3[] vertices = new Vector3[outlinePoints.Count];
        Vector2[] uvs = new Vector2[outlinePoints.Count];

        float pieceWidth = pieceConfig.pieceWidth;
        float pieceHeight = pieceConfig.pieceHeight;

        float puzzleWidth = puzzleConfig.columns * pieceWidth;
        float puzzleHeight = puzzleConfig.columns * pieceHeight;

        for (int i = 0; i < outlinePoints.Count; i++)
        {
            Vector2 localPoint = outlinePoints[i];

            vertices[i] = new Vector3(localPoint.x, localPoint.y, 0f);

            float imageX = pieceData.Column * pieceWidth + localPoint.x;
            float imageY = pieceData.Row * pieceHeight + localPoint.y;

            float u = imageX / puzzleWidth;
            float v = 1f - (imageY / puzzleHeight);

            uvs[i] = new Vector2(u, v);
        }

        mesh.vertices = vertices;
        mesh.triangles = Triangulate(outlinePoints);
        mesh.uv=uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private int[] Triangulate(List<Vector2> outlinePoints)
    {
        //LibTessDotNet is the library/plugin I used here
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
