using Renderer3D.Models.Extensions;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.Data.Concurrency
{
    public class ReadOnlyConcurrentBitmap
    {
        protected readonly WriteableBitmap _bitmap;
        protected readonly IntPtr backBuffer;

        public int Stride { get; }
        public int Width { get; }
        public int Height { get; }
        public int BytesPerPixel { get; }
        public WriteableBitmap WriteableBitmap => _bitmap;

        public ReadOnlyConcurrentBitmap(WriteableBitmap bitmap)
        {
            _bitmap = bitmap;
            Width = bitmap.PixelWidth;
            Height = bitmap.PixelHeight;
            backBuffer = bitmap.BackBuffer;
            Stride = bitmap.BackBufferStride;
            BytesPerPixel = bitmap.Format.BitsPerPixel / 8;
        }

        public Vector3 GetColor(int u, int v)
        {
            IntPtr pBackBuffer = backBuffer + v * Stride + u * BytesPerPixel;
            int color = Marshal.ReadInt32(pBackBuffer);
            return color.ToVector3();
        }
    }
}
