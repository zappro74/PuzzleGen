using System;
using System.Collections.Generic;
using System.Numerics;

//Just an FYI:
//t is the progress along the curve
//u is the inverse of t (so whats left of the curve)
//Bezier curves get pretty gnarly

namespace Puzzle.Geometry
{
    public static class BezierUtility
    {
        public static List<Vector2> CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int pieceCount)
        {
            if (pieceCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pieceCount), "pieceCount must be at least 1.");
            }

            List<Vector2> points = new List<Vector2>(pieceCount + 1);

            for (int i = 0; i < pieceCount; i++)
            {
                float t = i / (float)pieceCount;
                points.Add(EvaluateCubicBezier(p0, p1, p2, p3, t));
            }

            return points;
        }

        public static Vector2 EvaluateCubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            t = Math.Clamp(t, 0f, 1f);

            float u = 1f - t;

            float tSquared = t * t;
            float tCubed = tSquared * t;

            float uSquared = u * u;
            float uCubed = uSquared * u;

            return (uCubed * p0) + (3f * uSquared * t * p1) + (3f * u * tSquared * p2) + (tCubed * p3);
        }

        public static List<Vector2> GenerateEdgeCurve(Vector2 start, Vector2 end, EdgeType edgeType, float tabHeight, float edgeMargin, float tabWidth, int pointsPerCurveHalf)
        {
            if (pointsPerCurveHalf < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pointsPerCurveHalf), "pointsPerCurveHalf must be at least 1.");
            }

            if (edgeMargin < 0f || edgeMargin >= 0.5f)
            {
                throw new ArgumentOutOfRangeException(nameof(edgeMargin), "edgeMargin must be in [0, 0.5).");
            }

            if (tabWidth <= 0f || tabWidth >= 0.5f)
            {
                throw new ArgumentOutOfRangeException(nameof(tabWidth), "tabWidth must be in (0, 0.5).");
            }

            Vector2 edge = end - start;

            if (edge.Length() <= 0.0001f)
            {
                throw new ArgumentException("Start and end points must not be the same");
            }

            if (edgeType == EdgeType.Flat)
            {
                return new List<Vector2> { start, end };
            }

            Vector2 tangent = Vector2.Normalize(edge);
            Vector2 normal = new Vector2(-tangent.Y, tangent.X);

            float directionMultiplier = edgeType == EdgeType.Extruded ? 1f : -1f;
            
            Vector2 curve1Start = Vector2.Lerp(start, end, edgeMargin);
            Vector2 curve1End = Vector2.Lerp(start, end, 1f - edgeMargin);

            Vector2 mid = Vector2.Lerp(start, end, 0.5f);
            Vector2 Curve2Start = Vector2.Lerp(start, end, 0.5f - tabWidth);
            Vector2 Curve2End = Vector2.Lerp(start, end, 0.5f + tabWidth);

            Vector2 tabOffset = normal * (tabHeight * directionMultiplier);
            Vector2 peak = mid + tabOffset;

            Vector2[] curve1 = new Vector2[4];
            Vector2[] curve2 = new Vector2[4];

            curve1[0] = curve1Start;
            curve1[1] = Vector2.Lerp(curve1Start, Curve2Start, 0.5f);
            curve1[2] = Curve2Start + (tabOffset * 0.85f);
            curve1[3] = peak;

            curve2[0] = peak;
            curve2[1] = Curve2End + (tabOffset * 0.85f);
            curve2[2] = Vector2.Lerp(Curve2End, curve1End, 0.5f);
            curve1[3] = curve1End;

            List<Vector2> result = new List<Vector2>();

            result.Add(start);

            if (start != curve1Start)
            {
                result.Add(curve1Start);
            }

            List<Vector2> firstHalf = CubicBezier(curve1[0], curve1[1], curve1[2], curve1[3], pointsPerCurveHalf);
            result.AddRange(firstHalf);

            List<Vector2> secondHalf = CubicBezier(curve2[0], curve2[1], curve2[2], curve2[3], pointsPerCurveHalf);
            result.AddRange(secondHalf);

            if (curve1End != end)
            {
                result.Add(end);
            }

            return result;
        }
    }

}
