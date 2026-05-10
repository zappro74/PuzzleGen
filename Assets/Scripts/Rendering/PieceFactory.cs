using UnityEngine;
using System.Collections.Generic;
using LibTessDotNet;
public class PieceFactory : MonoBehaviour
{
    public GameObject CreatePiece(PieceData pieceData, PuzzleConfig puzzleConfig)
    {
        PieceConfig pieceConfig = puzzleConfig.pieceConfig;

        List<Vector2> outlinePoints = PieceOutline.BuildPieceOutline(pieceData, pieceConfig.pieceWidth, pieceConfig.pieceHeight, pieceConfig);

        //for testing
        //found out that all edges were generated as flat... pretty sure the issue could be in puzzle generator
        Debug.Log($"Piece {pieceData.Id} outline points: {outlinePoints.Count}");
        Debug.Log($"Edges: T={pieceData.TopEdge}, R={pieceData.RightEdge}, B={pieceData.BottomEdge}, L={pieceData.LeftEdge}");

        GameObject pieceObject = new GameObject($"Piece_{pieceData.Id}");

        MeshFilter meshFilter = pieceObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = pieceObject.AddComponent<MeshRenderer>();
        PolygonCollider2D polygonCollider = pieceObject.AddComponent<PolygonCollider2D>();
        PuzzlePiece puzzlePiece = pieceObject.AddComponent<PuzzlePiece>();
        
        Mesh pieceMesh = BuildMesh(outlinePoints, pieceData, puzzleConfig);
        meshFilter.mesh = pieceMesh;

        meshRenderer.material = pieceConfig.pieceMaterial;
        meshRenderer.material.mainTexture = puzzleConfig.puzzleImage;
        meshRenderer.sortingOrder = pieceData.Id;

        polygonCollider.points = outlinePoints.ToArray();

        puzzlePiece.Initialize(pieceData);

        return pieceObject;
    }

    private Mesh BuildMesh(List<Vector2> outlinePoints, PieceData pieceData, PuzzleConfig puzzleConfig)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices;
        int[] triangles = Triangulate(outlinePoints, out vertices);

        System.Array.Reverse(triangles);

        Vector2[] uvs = new Vector2[vertices.Length];

        PieceConfig pieceConfig = puzzleConfig.pieceConfig;

        float pieceWidth = pieceConfig.pieceWidth;
        float pieceHeight = pieceConfig.pieceHeight;

        float puzzleWidth = puzzleConfig.columns * pieceWidth;
        float puzzleHeight = puzzleConfig.rows * pieceHeight;

        for (int i = 0; i < vertices.Length; i++)
        {
            float localX = vertices[i].x;
            float localY = vertices[i].y;

            float imageX = pieceData.Column * pieceWidth + localX;
            float imageY = (puzzleConfig.rows - 1 - pieceData.Row) * pieceHeight + localY;

            float u = imageX / puzzleWidth;
            float v = imageY / puzzleHeight;

            uvs[i] = new Vector2(u, v);
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private int[] Triangulate(List<Vector2> outlinePoints, out Vector3[] vertices)
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

        vertices = new Vector3[tess.Vertices.Length];

        for (int i = 0; i < tess.Vertices.Length; i++)
        {
            vertices[i] = new Vector3(tess.Vertices[i].Position.X, tess.Vertices[i].Position.Y, 0f);
        }

        return tess.Elements;
    }
}
