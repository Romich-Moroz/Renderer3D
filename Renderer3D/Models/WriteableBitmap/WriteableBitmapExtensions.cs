using Renderer3D.Models.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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


        private static Point GetInsersectionPoint(Point A, Point B, Point C, Point D)
        {
            // Line AB represented as a1x + b1y = c1
            double a = B.Y - A.Y;
            double b = A.X - B.X;
            double c = a * (A.X) + b * (A.Y);
            // Line CD represented as a2x + b2y = c2
            double a1 = D.Y - C.Y;
            double b1 = C.X - D.X;
            double c1 = a1 * (C.X) + b1 * (C.Y);
            double det = a * b1 - a1 * b;
            if (det == 0)
            {
                return new Point(float.MaxValue, float.MaxValue);
            }
            else
            {
                double x = (b1 * c - b * c1) / det;
                double y = (a * c1 - a1 * c) / det;
                return new Point(x, y);
            }
        }

        private static void RasterizeTriangle(IntPtr backBuffer, int pixelWidth, int pixelHeight, int stride, Color color, Triangle tr)
        {
            var xMax = (int)Math.Max(Math.Max(tr.X1.X, tr.X2.X), tr.X3.X);
            var yMax = (int)Math.Max(Math.Max(tr.X1.Y, tr.X2.Y), tr.X3.Y);

            var xMin = (int)Math.Min(Math.Min(tr.X1.X, tr.X2.X), tr.X3.X);
            var yMin = (int)Math.Min(Math.Min(tr.X1.Y, tr.X2.Y), tr.X3.Y);            

            var max = tr.X1.Y == yMax ? tr.X1 : tr.X2.Y == yMax ? tr.X2 : tr.X3;
            var min = tr.X1.Y == yMin ? tr.X1 : tr.X2.Y == yMin ? tr.X2 : tr.X3;
            var middle = tr.X1 != max && tr.X1 != min ? tr.X1 : tr.X2 != max && tr.X2 != min ? tr.X2 : tr.X3;

            for (int y=yMax;y>=yMin;y--)
            {
                if ((xMin >= 0 && y >= 0) && (xMax < pixelWidth && y < pixelHeight))
                {
                    var x1 = new Point { X = xMin, Y = y };
                    var x2 = new Point { X = xMax, Y = y };
                    var p1 = GetInsersectionPoint(x1, x2, min, max);
                    var p2 = GetInsersectionPoint(x1, x2, middle, max);
                    DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, p1, p2);
                }
                else
                {
                    break;
                }               
            }
        }

        private static void DrawTriangle(IntPtr backBuffer, int pixelWidth, int pixelHeight, int stride, Color color, Triangle triangle)
        {
            DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, triangle.X1, triangle.X2);
            DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, triangle.X2, triangle.X3);
            DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, triangle.X3, triangle.X1);
            //RasterizeTriangle(backBuffer, stride, pixelWidth, pixelHeight, Colors.Gray, triangle);
        }

        /// <summary>
        /// Draws polygon without triangulation
        /// </summary>
        private static void DrawPolygon(IntPtr backBuffer, int pixelWidth, int pixelHeight, int stride, Color color, Polygon p, Point[] vertices, bool drawTriangles = false)
        {
            if (drawTriangles)
            {
                for (int i = 0; i < p.TriangleIndexes.Length; i++)
                {
                    DrawTriangle
                    (
                        backBuffer,
                        pixelWidth,
                        pixelHeight,
                        stride,
                        color,
                        new Triangle
                        {
                            X1 = vertices[p.TriangleIndexes[i].IndexX1],
                            X2 = vertices[p.TriangleIndexes[i].IndexX2],
                            X3 = vertices[p.TriangleIndexes[i].IndexX3]
                        }
                    );
                }
            }
            else
            {
                for (int i = 0; i < p.Vertices.Length; i++)
                {
                    if (i < p.Vertices.Length - 1)
                    {
                        DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, vertices[p.Vertices[i].VertexIndex], vertices[p.Vertices[i + 1].VertexIndex]);
                    }
                    else
                    {
                        DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, vertices[p.Vertices[^1].VertexIndex], vertices[p.Vertices[0].VertexIndex]);
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

        public static void DrawPolygons(this WriteableBitmap bitmap, Polygon[] polygons, Point[] vertices, Color color)
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
                        DrawPolygon(backBuffer, pixelWidth, pixelHeight, stride, color, polygons[i], vertices, true);
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
