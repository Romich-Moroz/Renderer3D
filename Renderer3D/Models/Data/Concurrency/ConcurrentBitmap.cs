using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.Data.Concurrency
{
    public class ConcurrentBitmap : ReadOnlyConcurrentBitmap
    {
        private readonly byte[] _blankBuffer;
        private readonly float[] _depthBuffer;
        private readonly object[] _lockBuffer;

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

        private DrawResult DrawPixel(int x, int y, int color, bool useDepthBuffer = false, float z = 0)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                IntPtr pBackBuffer = backBuffer + y * Stride + x * BytesPerPixel;
                int index = x + y * Width;

                lock (_lockBuffer[index])
                {
                    if (useDepthBuffer)
                    {
                        if (_depthBuffer[index] < z)
                        {
                            return DrawResult.DepthBufferOverlap;
                        }
                        _depthBuffer[index] = z;
                    }

                    unsafe
                    {
                        *(int*)pBackBuffer = color;
                    }
                }
                return DrawResult.Success;
            }
            return DrawResult.OutOfBounds;
        }

        public ConcurrentBitmap(WriteableBitmap bitmap) : base(bitmap)
        {
            int bitmapLength = bitmap.PixelWidth * bitmap.PixelHeight * 4;
            _blankBuffer = new byte[bitmapLength];
            unsafe
            {
                fixed (byte* b = _blankBuffer)
                {
                    Unsafe.InitBlock(b, 255, (uint)_blankBuffer.Length);
                    CopyMemory(bitmap.BackBuffer, (IntPtr)b, (uint)_blankBuffer.Length);
                }
            }
            _depthBuffer = new float[bitmap.PixelWidth * bitmap.PixelHeight];
            _lockBuffer = new object[bitmap.PixelWidth * bitmap.PixelHeight];
            for (int i = 0; i < _lockBuffer.Length; i++)
            {
                _lockBuffer[i] = new object();
            }
        }

        public DrawResult DrawPixel(int x, int y, float z, int color)
        {
            return DrawPixel(x, y, color, true, z);
        }

        public DrawResult DrawPixel(int x, int y, int color)
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
                if (DrawPixel((int)x, (int)y, color) == DrawResult.OutOfBounds)
                {
                    return;
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _depthBuffer.Length; i++)
            {
                _depthBuffer[i] = float.MaxValue;
            }

            try
            {
                unsafe
                {
                    fixed (byte* b = _blankBuffer)
                    {
                        CopyMemory(backBuffer, (IntPtr)b, (uint)_blankBuffer.Length);
                    }
                }

                _bitmap.Lock();
                _bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            }
            finally
            {
                _bitmap.Unlock();
            }
        }


    }
}
