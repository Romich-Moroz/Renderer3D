using System;
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

        private float ClampX => Width - 1.0f;
        private float ClampY => Height - 1.0f;

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

        public int GetColor(float u, float v)
        {
            float x = Math.Min(u * Width, ClampX);
            float y = Math.Min(v * Height, ClampY);
            IntPtr pBackBuffer = backBuffer + (int)y * Stride + (int)x * BytesPerPixel;
            return Marshal.ReadInt32(pBackBuffer);

        }
    }
}
