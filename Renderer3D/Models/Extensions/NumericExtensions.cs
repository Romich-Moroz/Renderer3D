using System.Numerics;
using System.Windows;

namespace Renderer3D.Models.Extensions
{
    public static class NumericExtensions
    {
        public static Point ToPoint(this Vector3 v)
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
