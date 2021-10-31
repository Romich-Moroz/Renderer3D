using Renderer3D.Models.Data;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.Processing
{
    public class BitmapWriter
    {
        private static byte[] _blankBuffer;
        private WriteableBitmap _bitmap;
        private float[] _depthBuffer;
        private object[] _lockBuffer;
        private IntPtr backBuffer;
        private int bytesPerPixel;

        public int Stride { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// Represents bitmap this writer is using
        /// </summary>
        public WriteableBitmap Bitmap
        {
            get => _bitmap;
            set
            {
                int bitmapLength = value.PixelWidth * value.PixelHeight * 4;

                Width = value.PixelWidth;
                Height = value.PixelHeight;
                backBuffer = value.BackBuffer;
                Stride = value.BackBufferStride;
                _blankBuffer = new byte[bitmapLength];
                bytesPerPixel = value.Format.BitsPerPixel / 8;
                unsafe
                {
                    fixed (byte* b = _blankBuffer)
                    {
                        Unsafe.InitBlock(b, 255, (uint)_blankBuffer.Length);
                        CopyMemory(value.BackBuffer, (IntPtr)b, (uint)_blankBuffer.Length);
                    }
                }
                _depthBuffer = new float[value.PixelWidth * value.PixelHeight];
                _lockBuffer = new object[value.PixelWidth * value.PixelHeight];
                for (int i = 0; i < _lockBuffer.Length; i++)
                {
                    _lockBuffer[i] = new object();
                }
                _bitmap = value;
            }
        }

        private DrawError DrawPixel(int x, int y, int color, bool useDepthBuffer = false, float z = 0)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                IntPtr pBackBuffer = backBuffer + y * Stride + x * bytesPerPixel;
                int index = x + y * Width;

                lock (_lockBuffer[index])
                {
                    if (useDepthBuffer)
                    {
                        if (_depthBuffer[index] < z)
                        {
                            return DrawError.DepthBufferOverlap;
                        }
                        _depthBuffer[index] = z;
                    }

                    unsafe
                    {
                        *(int*)pBackBuffer = color;
                    }
                }
                return DrawError.Success;
            }
            return DrawError.OutOfBounds;
        }

        public void Clear()
        {
            for (int i = 0; i < _depthBuffer.Length; i++)
            {
                _depthBuffer[i] = float.MaxValue;
            }

            try
            {
                // Reserve the back buffer for updates.
                unsafe
                {
                    fixed (byte* b = _blankBuffer)
                    {
                        CopyMemory(backBuffer, (IntPtr)b, (uint)_blankBuffer.Length);
                    }
                }

                Bitmap.Lock();
                Bitmap.AddDirtyRect(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight));
            }
            finally
            {
                Bitmap.Unlock();
            }
        }

        public DrawError DrawPixel(int x, int y, float z, int color)
        {
            return DrawPixel(x, y, color, true, z);
        }

        public DrawError DrawPixel(int x, int y, int color)
        {
            return DrawPixel(x, y, color, false, 0);
        }

        public void DrawLine(Point x1, Point x2, int color)
        {
            DdaStruct dda = DdaStruct.FromPoints(x1, x2);

            for (int i = 0; i < dda.LineLength; i++)
            {
                double x = x1.X + i * dda.DeltaX;
                double y = x1.Y + i * dda.DeltaY;
                if (DrawPixel((int)x, (int)y, color) == DrawError.OutOfBounds)
                {
                    return;
                }
            }
        }


        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);
    }
}
