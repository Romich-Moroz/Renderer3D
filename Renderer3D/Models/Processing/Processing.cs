using Renderer3D.Models.Data;
using System;
using System.Numerics;

namespace Renderer3D.Models.Processing
{
    public static class Processing
    {
        public static float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        /// <summary>
        /// Interpolates the value between 2 vertices 
        /// </summary>
        /// <param name="min">Starting point</param>
        /// <param name="max">Ending point</param>
        /// <param name="gradient">The % between the 2 points</param>
        /// <returns></returns>
        public static float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        public static float ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            Vector3 lightDirection = lightPosition - vertex;
            return Math.Max(0, -Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(lightDirection)));
        }

        public static bool IsTriangleInvisible(Triangle t)
        {
            return (t.v0.Coordinates.Y == t.v1.Coordinates.Y && t.v0.Coordinates.Y == t.v2.Coordinates.Y) ||
                (Vector3.Cross(t.v1.Coordinates - t.v0.Coordinates, t.v2.Coordinates - t.v0.Coordinates).Z >= 0);
        }

        public static void SortTriangleVerticesByY(ref Triangle t)
        {
            if (t.v0.Coordinates.Y > t.v1.Coordinates.Y)
            {
                (t.v0, t.v1) = (t.v1, t.v0);
            }

            if (t.v0.Coordinates.Y > t.v2.Coordinates.Y)
            {
                (t.v0, t.v2) = (t.v2, t.v0);
            }

            if (t.v1.Coordinates.Y > t.v2.Coordinates.Y)
            {
                (t.v1, t.v2) = (t.v2, t.v1);
            }
        }

        public static (double, double) GetInverseSlopes(Triangle t)
        {
            double dP1P2, dP1P3;
            if (t.v1.Coordinates.Y - t.v0.Coordinates.Y > 0)
            {
                dP1P2 = (t.v1.Coordinates.X - t.v0.Coordinates.X) / (t.v1.Coordinates.Y - t.v0.Coordinates.Y);
            }
            else
            {
                dP1P2 = 0;
            }

            if (t.v2.Coordinates.Y - t.v0.Coordinates.Y > 0)
            {
                dP1P3 = (t.v2.Coordinates.X - t.v0.Coordinates.X) / (t.v2.Coordinates.Y - t.v0.Coordinates.Y);
            }
            else
            {
                dP1P3 = 0;
            }
            return (dP1P2, dP1P3);
        }

    }
}
