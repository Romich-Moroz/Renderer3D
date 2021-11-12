using System.Numerics;
using System.Windows;

namespace Renderer3D.Models.Extensions
{
    public static class NumericExtensions
    {
        private static readonly Vector3 Min = new Vector3(0, 0, 0);
        private static readonly Vector3 Max = new Vector3(1, 1, 1);

        public static Vector3 ToColor(this Vector3 color)
        {
            return Vector3.Clamp(color, Min, Max) * 255;
        }

        public static Point ToPoint(this Vector4 v)
        {
            return new Point(v.X, v.Y);
        }

        public static Vector3 ToV3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static int ToInt(this Vector3 vector)
        {
            return ((int)vector.X << 16) | ((int)vector.Y << 8) | ((int)vector.Z << 0);
        }

        public static Vector3 ToVector3(this int number)
        {
            return new Vector3
            {
                X = (number >> 16) & 0xff,
                Y = (number >> 8) & 0xff,
                Z = number & 0xff
            };
        }
    }
}
