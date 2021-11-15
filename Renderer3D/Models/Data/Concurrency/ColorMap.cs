using Renderer3D.Models.Extensions;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.Data.Concurrency
{
    public class ColorMap
    {
        private readonly Vector3[,] Colors;
        public readonly int Width;
        public readonly int Height;

        public Vector3 GetColor(float u, float v)
        {
            u = u > 0 ? u % 1f : u - (float)Math.Floor(u);
            v = v > 0 ? v % 1f : v - (float)Math.Floor(v);

            int x = (int)(u * Width);
            int y = (int)(v * Height);

            return Colors[y, x];
        }

        public ColorMap(WriteableBitmap bitmap)
        {
            Width = bitmap.PixelWidth;
            Height = bitmap.PixelHeight;
            Colors = new Vector3[Width, Height];
            int bytesPerPixel = bitmap.Format.BitsPerPixel / 8;

            IntPtr pBackBuffer = bitmap.BackBuffer;
            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++, pBackBuffer += bytesPerPixel)
                {
                    Colors[y, x] = Marshal.ReadInt32(pBackBuffer).ToVector3() / 255;
                }
            }
        }
    }
}
