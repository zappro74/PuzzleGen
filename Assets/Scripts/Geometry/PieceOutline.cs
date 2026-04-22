using System.Collections.Generic;
using System.Drawing;
using Puzzle.Geometry;
using Unity.VisualScripting;
using UnityEngine;

public class PieceOutline
{
    public static List<Vector2> BuildPieceOutline(PieceData piece, float pieceWidth, float pieceHeight, float tabWidth, float tabHeight, float edgeMargin, int pointsPerCurveHalf)
    {
        Vector2 topLeft = new Vector2(0, 0);
        Vector2 topRight = new Vector2(pieceWidth, 0);
        Vector2 bottomRight = new Vector2(pieceWidth, pieceHeight);
        Vector2 bottomLeft = new Vector2(0, pieceHeight);

        List<Vector2> topEdge = BezierUtility.GenerateEdgeCurve(topLeft, topRight, piece.TopEdge, tabHeight, edgeMargin, tabWidth, pointsPerCurveHalf);
        List<Vector2> rightEdge = BezierUtility.GenerateEdgeCurve(topRight, bottomRight, piece.RightEdge, tabHeight, edgeMargin, tabWidth, pointsPerCurveHalf);
        List<Vector2> bottomEdge = BezierUtility.GenerateEdgeCurve(bottomRight, bottomLeft, piece.BottomEdge, tabHeight, edgeMargin, tabWidth, pointsPerCurveHalf);
        List<Vector2> leftEdge = BezierUtility.GenerateEdgeCurve(bottomLeft, topLeft, piece.LeftEdge, tabHeight, edgeMargin, tabWidth, pointsPerCurveHalf);

        List<Vector2> outline = new List<Vector2>();

        outline.AddRange(topEdge);
        AddEdge(outline, rightEdge);
        AddEdge(outline, bottomEdge);
        AddEdge(outline, leftEdge); //might need to reamove last point

        return outline;
    }
    
    //Removes the first point of the edge
    //otherwise would count the last point and would make a duplicate
    private static void AddEdge(List<Vector2> outline, List<Vector2> edge)
    {
        for (int i = 1; i < edge.Count; i++)
        {
            outline.Add(edge[i]);
        }
    }
}
