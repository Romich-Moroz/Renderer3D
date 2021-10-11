using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace Renderer3D.Models.Extensions
{
    public static class NumericExtensions
    {
        public static Point ToPoint(this Vector3 v)
        {
            return new Point(v.X, v.Y);
        }

        public static Point ToPoint(this Vector4 v)
        {
            return new Point(v.X, v.Y);
        }

        public static Vector3 ToV3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static int ToInt(this Color color)
        {
            return (color.R << 16) | (color.G << 8) | (color.B << 0);
        }

        public static Color ToColor(this int color)
        {
            return Color.FromRgb((byte)((color >> 16) & 0xff), (byte)((color >> 8) & 0xff), (byte)(color & 0xff));
        }
    }
}
