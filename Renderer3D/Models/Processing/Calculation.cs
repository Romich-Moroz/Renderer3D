using Renderer3D.Models.Data;
using Renderer3D.Models.Data.Properties;
using Renderer3D.Models.Extensions;
using Renderer3D.Models.Processing.Shaders;
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
                (Vector3.Cross((t.v1.Coordinates - t.v0.Coordinates).ToV3(), (t.v2.Coordinates - t.v0.Coordinates).ToV3()).Z >= 0);
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
            dP1P2 = v1.Y - v0.Y > 0 ? (v1.X - v0.X) / (v1.Y - v0.Y) : 0;

            dP1P3 = v2.Y - v0.Y > 0 ? (v2.X - v0.X) / (v2.Y - v0.Y) : 0;
            return (dP1P2, dP1P3);
        }

        public static Vector3 GetDiffuseLightingColor(MaterialProperties materialProperties, Vector3 light, RenderStruct renderStruct)
        {
            return renderStruct.DiffuseColor * ComputeNDotL(light, renderStruct.Normal) * materialProperties.DiffuseColorIntensity;
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

        public static Vector3 GetSpecularColor(MaterialProperties materialProperties, Vector3 hVector, RenderStruct renderStruct)
        {
            float dot = Math.Abs(Vector3.Dot(renderStruct.Normal, hVector));
            float pow = Pow(dot, (int)materialProperties.SpecularHighlight);
            return renderStruct.SpecularColor * pow * materialProperties.SpecularColorIntensity;
        }

    }
}
