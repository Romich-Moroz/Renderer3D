using System.Numerics;

namespace Renderer3D.Models.Translation
{
    /// <summary>
    /// Matrix translation library
    /// </summary>
    public static class Translator
    {
        /// <summary>
        /// Creates viewport matrix for transformation
        /// </summary>
        /// <param name="width">Width of the screen</param>
        /// <param name="height">Height of the screen</param>
        /// <param name="xMin">Min screen coordinate of x axis</param>
        /// <param name="yMin">Min screen coordinate of y axis</param>
        /// <returns>Viewport patrix for translation</returns>
        public static Matrix4x4 CreateViewportMatrix(float width, float height, int xMin = 0, int yMin = 0)
        {
            return new Matrix4x4
            {
                M11 = width / 2,
                M12 = 0,
                M13 = 0,
                M14 = 0,
                M21 = 0,
                M22 = -height / 2,
                M23 = 0,
                M24 = 0,
                M31 = 0,
                M32 = 0,
                M33 = 1,
                M34 = 0,
                M41 = xMin + width / 2,
                M42 = yMin + height / 2,
                M43 = 0,
                M44 = 1
            };
        }

        public static Vector4 PerspectiveDivide(this Vector4 vector)
        {
            return new Vector4 { X = vector.X / vector.W, Y = vector.Y / vector.W, Z = vector.Z / vector.W, W = 1 };
        }
    }
}
