using Renderer3D.Models.Data;
using Renderer3D.Models.Extensions;
using System;
using System.Numerics;
using System.Windows.Media;

namespace Renderer3D.Models.Processing
{
    public static class Calculation
    {
        public static float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        public static int MultiplyColorByFloat(Color color, float multiplier)
        {
            return Color.FromRgb((byte)(color.R * multiplier), (byte)(color.G * multiplier), (byte)(color.B * multiplier)).ToInt();
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

        public static float ComputeNDotL(Vector3 lightDirection, Vector3 normal)
        {
            return Math.Max(0, -Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(lightDirection)));
        }

        public static bool IsTriangleInvisible(TriangleValue t)
        {
            return (t.v0.Coordinates.Y == t.v1.Coordinates.Y && t.v0.Coordinates.Y == t.v2.Coordinates.Y) ||
                (Vector3.Cross(t.v1.Coordinates.ToV3() - t.v0.Coordinates.ToV3(), t.v2.Coordinates.ToV3() - t.v0.Coordinates.ToV3()).Z >= 0);
        }

        public static void SortTriangleVerticesByY(ref TriangleValue t)
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

        public static (double, double) GetInverseSlopes(Vector4 v0, Vector4 v1, Vector4 v2)
        {
            double dP1P2, dP1P3;
            if (v1.Y - v0.Y > 0)
            {
                dP1P2 = (v1.X - v0.X) / (v1.Y - v0.Y);
            }
            else
            {
                dP1P2 = 0;
            }

            if (v2.Y - v0.Y > 0)
            {
                dP1P3 = (v2.X - v0.X) / (v2.Y - v0.Y);
            }
            else
            {
                dP1P3 = 0;
            }
            return (dP1P2, dP1P3);
        }

        public static Vector3 InterpolateNormal(Vector3 normal1, Vector3 normal2, float interpolationParameter)
        {
            return (1 - interpolationParameter) * normal1 + interpolationParameter * normal2;
        }

    }
}
