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
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            return (uuu * p0) + (3f * uu * t * p1) + (3f * u * tt * p2) + (ttt * p3);
        }
    }

}
