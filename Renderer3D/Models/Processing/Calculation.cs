﻿using Renderer3D.Models.Data;
using Renderer3D.Models.Data.Properties;
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

        public static Color Multiply(this Color color, float mult)
        {
            color.R = (byte)(color.R * mult);
            color.G = (byte)(color.G * mult);
            color.B = (byte)(color.B * mult);
            return color;
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
                (Vector3.Cross(t.v1.Coordinates - t.v0.Coordinates, t.v2.Coordinates - t.v0.Coordinates).Z >= 0);
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

        public static (double, double) GetInverseSlopes(Vector3 v0, Vector3 v1, Vector3 v2)
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

        /// <summary>
        /// More accurate than GetFastBarycentricCoordinates but slower
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Vector3 GetBarycentricCoordinates(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            float sabp = Vector3.Cross(p - a, p - b).Length();
            float sacp = Vector3.Cross(p - a, p - c).Length();
            float denom = Vector3.Cross(a - b, a - c).Length();

            float w = sabp / denom;
            float v = sacp / denom;
            float u = 1 - w - v;

            return new Vector3
            {
                X = u,
                Y = v,
                Z = w
            };
        }

        /// <summary>
        /// Faster than GetBarycentricCoordinates but less accurate
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Vector3 GetFastBarycentricCoordinates(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);
            float invDenom = 1.0f / (d00 * d11 - d01 * d01);

            float v = (d11 * d20 - d01 * d21) * invDenom;
            float w = (d00 * d21 - d01 * d20) * invDenom;
            float u = 1.0f - v - w;

            return new Vector3
            {
                X = u,
                Y = v,
                Z = w
            };
        }

        public static Vector3 GetDiffuseLightingColor(MaterialProperties materialProperties, Vector3 light, Vector3 normal)
        {
            Vector3 Id = new Vector3(0xD4, 0XAF, 0x37); //Replace with actual texture color
            return Id * ComputeNDotL(light, normal) * materialProperties.DiffuseColorIntensity;
        }

        public static float Pow(float value, int pow)
        {
            float result = 1.0f;
            while (pow > 0)
            {
                if (pow % 2 == 1)
                {
                    result *= value;
                }

                pow >>= 1;
                value *= value;
            }

            return result;
        }

        public static Vector3 GetSpecularColor(MaterialProperties materialProperties, Vector3 hVector, Vector3 normal)
        {
            float dot = Math.Abs(Vector3.Dot(normal, hVector));
            float pow = Pow(dot, (int)materialProperties.SpecularHighlight);

            Vector3 Is = new Vector3(0xFF, 0xCF, 0x42); //Replace with actual reflection color
            return Is * pow * materialProperties.SpecularColorIntensity;
        }
    }
}
