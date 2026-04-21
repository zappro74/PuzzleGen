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

        public static List<Vector2> GenerateEdgeCurve(Vector2 start, Vector2 end, EdgeType edgeType, float tabHeight, float shoulderRatio, float neckRatio, int sampleCountPerHalf)
        {
            if (sampleCountPerHalf < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleCountPerHalf), "sampleCountPerHalf must be at least 1.");
            }

            if (shoulderRatio < 0f || shoulderRatio >= 0.5f)
            {
                throw new ArgumentOutOfRangeException(nameof(shoulderRatio), "shoulderRatio must be in [0, 0.5).");
            }

            if (neckRatio <= 0f || neckRatio >= 0.5f)
            {
                throw new ArgumentOutOfRangeException(nameof(neckRatio), "neckRatio must be in (0, 0.5).");
            }

            
        }
    }

}
