using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

            for (int i = 0; i <= pieceCount; i++)
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

        public static List<Vector2> GenerateEdgeCurve(Vector2 start, Vector2 end, EdgeType edgeType, Vector2 outwardNormal, float tabHeight, float edgeMargin, float tabWidth, int pointsPerCurveHalf)
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
            float edgeLength = edge.magnitude;

            if (edge.magnitude <= 0.0001f)
            {
                throw new ArgumentException("Start and end points must not be the same");
            }

            if (edgeType == EdgeType.Flat)
            {
                return new List<Vector2> { start, end };
            }

            Vector2 tangent = edge.normalized;
            Vector2 normal = outwardNormal.normalized;

            float tabWorldWidth = edgeLength * tabWidth;

            float directionMultiplier = edgeType == EdgeType.Extruded ? 1f : -1f;
            Vector2 tabOffset = normal * (tabHeight * directionMultiplier);
            
            //the middle portion of the edge not including the corners
            Vector2 usableStart = Vector2.Lerp(start, end, edgeMargin);
            Vector2 usableEnd = Vector2.Lerp(start, end, 1f - edgeMargin);

            Vector2 tabStart = Vector2.Lerp(usableStart, usableEnd, 0.5f - tabWidth);
            Vector2 tabEnd = Vector2.Lerp(usableStart, usableEnd, 0.5f + tabWidth);

            //0.35 my beloved
            Vector2 capLeft = Vector2.Lerp(usableStart, usableEnd, 0.5f - tabWidth * 0.35f) + tabOffset;
            Vector2 capRight = Vector2.Lerp(usableStart, usableEnd, 0.5f + tabWidth * 0.35f) + tabOffset;

            List<Vector2> result = new List<Vector2>();

            result.Add(start);
            result.Add(usableStart);
            result.Add(tabStart);

            AddCurve
            (
                result,
                CubicBezier(tabStart,
                tabStart + tangent * (tabWorldWidth * 0.25f), 
                capLeft - tangent * (tabWorldWidth * 0.25f), 
                capLeft, 
                pointsPerCurveHalf)
            );
            
            AddCurve
            (
                result,
                CubicBezier(
                capLeft,
                capLeft + tangent * (tabWorldWidth * 0.35f),
                capRight - tangent * (tabWorldWidth * 0.35f),
                capRight,
                pointsPerCurveHalf)
            );

            AddCurve
            (
                result,
                CubicBezier(
                capRight,
                capRight + tangent * (tabWorldWidth * 0.25f),
                tabEnd - tangent * (tabWorldWidth * 0.25f),
                tabEnd,
                pointsPerCurveHalf)
            );

            result.Add(tabEnd);
            result.Add(usableEnd);
            result.Add(end);

            return result;
        }

        private static void AddCurve(List<Vector2> result, List<Vector2> curve)
        {
            for (int i = 1; i < curve.Count; i++)
            {
                result.Add(curve[i]);
            }
        }
    }
}
