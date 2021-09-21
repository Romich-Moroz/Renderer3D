using Renderer3D.Models.Data;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.WritableBitmap
{
    /// <summary>
    /// Drawing extensions for writeable bitmap
    /// </summary>
    public static class WriteableBitmapExtensions
    {
        private static byte[] _blankBuffer;
        private static float[] _depthBuffer;

        private static void DrawLine(IntPtr backBuffer, int stride, int pixelWidth, int pixelHeight, Color color, Point x1, Point x2)
        {
            double x2x1 = x2.X - x1.X;
            double y2y1 = x2.Y - x1.Y;
            double l = Math.Abs(x2x1) > Math.Abs(y2y1) ? Math.Abs(x2x1) : Math.Abs(y2y1);
            double xDelta = x2x1 / l;
            double yDelta = y2y1 / l;

            int color_data = color.ToInt();

            unsafe
            {
                for (int i = 0; i < l; i++)
                {
                    double x = x1.X + i * xDelta;
                    double y = x1.Y + i * yDelta;
                    if ((x >= 0 && y >= 0) && (x < pixelWidth && y < pixelHeight))
                    {
                        // Find the address of the pixel to draw.
                        IntPtr pBackBuffer = backBuffer + (int)y * stride + (int)x * 4;

                        // Assign the color data to the pixel.
                        *((int*)pBackBuffer) = color_data;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private static void DrawPoint(IntPtr backBuffer, int stride, int pixelWidth, int pixelHeight, Color color, Vector3 point)
        {
            
        }

        private static Point ToPoint(this Vector3 v)
        {
            return new Point(v.X, v.Y);
        }

        private static float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        // Interpolating the value between 2 vertices 
        // min is the starting point, max the ending point
        // and gradient the % between the 2 points
        private static float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        private static void ProcessScanLine(IntPtr backBuffer, int pixelWidth, int pixelHeight, int stride, Color color, int y, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd)
        {
            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            float gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            float gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);
            // starting Z & ending Z
            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);

                if (x >= 0 && y >= 0 && x < pixelWidth && y < pixelHeight)
                {
                    IntPtr pBackBuffer = backBuffer + (int)y * stride + (int)x * 4;
                    var index = ((int)x + (int)y * pixelWidth);
                    var index4 = index * 4;
                    int color_data = color.ToInt();

                    if (_depthBuffer[index] < z)
                    {
                        return; // Discard
                    }
                    _depthBuffer[index] = z;

                    unsafe
                    {
                        *((int*)pBackBuffer) = color_data;
                    }
                }
            }
        }

        private static void DrawTriangle(IntPtr backBuffer, int pixelWidth, int pixelHeight, int stride, Color color, Triangle t)
        {

            if (t.p1.Y == t.p2.Y && t.p1.Y == t.p3.Y)
            {
                return; // i dont care about degenerate triangles
            }

            if (t.p1.Y > t.p2.Y)
            {
                (t.p1, t.p2) = (t.p2, t.p1);
            }

            if (t.p1.Y > t.p3.Y)
            {
                (t.p1, t.p3) = (t.p3, t.p1);
            }

            if (t.p2.Y > t.p3.Y)
            {
                (t.p2, t.p3) = (t.p3, t.p2);
            }

            double dP1P2, dP1P3;
            if (t.p2.Y - t.p1.Y > 0)
            {
                dP1P2 = (t.p2.X - t.p1.X) / (t.p2.Y - t.p1.Y);
            }
            else
            {
                dP1P2 = 0;
            }

            if (t.p3.Y - t.p1.Y > 0)
            {
                dP1P3 = (t.p3.X - t.p1.X) / (t.p3.Y - t.p1.Y);
            }
            else
            {
                dP1P3 = 0;
            }

            for (int y = (int)t.p1.Y; y <= (int)t.p3.Y; y++)
            {
                if (y < t.p2.Y)
                {
                    ProcessScanLine(backBuffer, pixelWidth, pixelHeight, stride, Colors.Gray, y, t.p1, dP1P2 > dP1P3 ? t.p3 : t.p2, t.p1, dP1P2 > dP1P3 ? t.p2 : t.p3);
                }
                else
                {
                    ProcessScanLine(backBuffer, pixelWidth, pixelHeight, stride, Colors.Gray, y, dP1P2 > dP1P3 ? t.p1 : t.p2, t.p3, dP1P2 > dP1P3 ? t.p2 : t.p1, t.p3);
                }
            }

            //DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, t.p1.ToPoint(), t.p2.ToPoint());
            //DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, t.p2.ToPoint(), t.p3.ToPoint());
            //DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, t.p3.ToPoint(), t.p1.ToPoint());
        }

        /// <summary>
        /// Draws polygon without triangulation
        /// </summary>
        private static void DrawPolygon(IntPtr backBuffer, int pixelWidth, int pixelHeight, int stride, Color color, Polygon p, Vector3[] vertices, bool drawTriangles = false)
        {
            if (drawTriangles)
            {
                for (int i = 0; i < p.TriangleIndexes.Length; i++)
                {
                    var triangle = new Triangle
                    {
                        p1 = vertices[p.TriangleIndexes[i].IndexX1],
                        p2 = vertices[p.TriangleIndexes[i].IndexX2],
                        p3 = vertices[p.TriangleIndexes[i].IndexX3]
                    };
                    DrawTriangle
                    (
                        backBuffer,
                        pixelWidth,
                        pixelHeight,
                        stride,
                        color,
                        triangle
                    );
                }
            }
            else
            {
                for (int i = 0; i < p.Vertices.Length; i++)
                {
                    if (i < p.Vertices.Length - 1)
                    {
                        DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, vertices[p.Vertices[i].VertexIndex].ToPoint(), vertices[p.Vertices[i + 1].VertexIndex].ToPoint());
                    }
                    else
                    {
                        DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, vertices[p.Vertices[^1].VertexIndex].ToPoint(), vertices[p.Vertices[0].VertexIndex].ToPoint());
                    }
                }
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

        public static int ToInt(this Color color)
        {
            return color.R << 16 | color.G << 8 | color.B << 0;
        }

        public static void Clear(this WriteableBitmap bitmap)
        {
            int bitmapLength = bitmap.PixelWidth * bitmap.PixelHeight * 4;
            if (_blankBuffer == null || _blankBuffer.Length != bitmapLength)
            {
                _blankBuffer = new byte[bitmapLength];
                _depthBuffer = new float[bitmap.PixelWidth * bitmap.PixelHeight];
            }

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
                        System.Runtime.CompilerServices.Unsafe.InitBlock(b, 255, (uint)_blankBuffer.Length);
                        CopyMemory(bitmap.BackBuffer, (IntPtr)b, (uint)_blankBuffer.Length);
                    }
                }

                bitmap.Lock();
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        public static void DrawPolygons(this WriteableBitmap bitmap, Polygon[] polygons, Vector3[] vertices, Color color, bool drawTriangles)
        {
            int pixelWidth = bitmap.PixelWidth;
            int pixelHeight = bitmap.PixelHeight;
            IntPtr backBuffer = bitmap.BackBuffer;
            int stride = bitmap.BackBufferStride;

            try
            {
                Parallel.ForEach(Partitioner.Create(0, polygons.Length), Range =>
                {
                    for (int i = Range.Item1; i < Range.Item2; i++)
                    {
                        DrawPolygon(backBuffer, pixelWidth, pixelHeight, stride, color, polygons[i], vertices, drawTriangles);
                    }
                });
                bitmap.Lock();
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                bitmap.Unlock();
            }
        }

    }
}
