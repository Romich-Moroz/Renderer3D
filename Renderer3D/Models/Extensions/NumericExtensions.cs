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

        public static int ToInt(this Color color)
        {
            return color.R << 16 | color.G << 8 | color.B << 0;
        }
    }
}
