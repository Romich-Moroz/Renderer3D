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

        private static void RasterizeTriangle(Triangle triangle)
        {

        }

        private static void DrawTriangle(IntPtr backBuffer, int pixelWidth, int pixelHeight, int stride, Color color, Triangle triangle)
        {
            DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, triangle.X1, triangle.X2);
            DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, triangle.X2, triangle.X3);
            DrawLine(backBuffer, stride, pixelWidth, pixelHeight, color, triangle.X3, triangle.X1);
        }

        private static Triangle[] SplitPolygon(Polygon p, Point[] vertices)
        {
            List<Triangle> result = new List<Triangle>();
            if (p.Vertices.Length == 3)
            {
                return new[]
                {
                    new Triangle
                    {
                        X1 = vertices[p.Vertices[0].VertexIndex],
                        X2 = vertices[p.Vertices[0].VertexIndex],
                        X3 = vertices[p.Vertices[0].VertexIndex]
                    }
                };
            }
            else
            {
                for (int i = 2; i < p.Vertices.Length; i++)
                {
                    result.Add(new Triangle
                    {
                        X1 = vertices[p.Vertices[0].VertexIndex],
                        X2 = vertices[p.Vertices[i - 1].VertexIndex],
                        X3 = vertices[p.Vertices[i].VertexIndex]
                    });
                }
                return result.ToArray();
            }
        }

        /// <summary>
        /// Draws polygon without triangulation
        /// </summary>
        private static void DrawPolygon(IntPtr backBuffer, int pixelWidth, int pixelHeight, int stride, Color color, Polygon p, Point[] vertices)
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

        private static void DrawPolygonTriangles(IntPtr backBuffer, int pixelWidth, int pixelHeight, int stride, Color color, Polygon p, Point[] vertices)
        {
            Triangle[] triangles = SplitPolygon(p, vertices);
            for (int i = 0; i < triangles.Length; i++)
            {
                DrawTriangle(backBuffer, pixelWidth, pixelHeight, stride, color, triangles[i]);
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

        //INLINED FOR OPTIMIZATION PURPOSES
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
                        DrawPolygonTriangles(backBuffer, pixelWidth, pixelHeight, stride, color, polygons[i], vertices);
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
