﻿using Renderer3D.Models.Data;
using System;
using System.Collections.Concurrent;
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
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

        public static int ToInt(this Color color)
        {
            return color.R << 16 | color.G << 8 | color.B << 0;
        }

        public static void DrawPixel(this WriteableBitmap bitmap, Point x, Color color)
        {
            int column = (int)x.X;
            int row = (int)x.Y;

            try
            {
                // Reserve the back buffer for updates.
                bitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = bitmap.BackBuffer;

                    // Find the address of the pixel to draw.
                    pBackBuffer += row * bitmap.BackBufferStride;
                    pBackBuffer += column * 4;

                    // Assign the color data to the pixel.
                    *((int*)pBackBuffer) = color.ToInt();
                }

                // Specify the area of the bitmap that changed.
                bitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                bitmap.Unlock();
            }
        }

        public static void DrawLine(this WriteableBitmap bitmap, Point x1, Point x2, Color color)
        {
            double x2x1 = x2.X - x1.X;
            double y2y1 = x2.Y - x1.Y;
            double l = Math.Abs(x2x1) > Math.Abs(y2y1) ? Math.Abs(x2x1) : Math.Abs(y2y1);
            double xDelta = x2x1 / l;
            double yDelta = y2y1 / l;

            try
            {
                // Reserve the back buffer for updates.
                bitmap.Lock();
                unsafe
                {
                    // Compute the pixel's color.
                    int color_data = color.ToInt();

                    for (int i = 0; i < l; i++)
                    {
                        double x = x1.X + i * xDelta;
                        double y = x1.Y + i * yDelta;
                        if ((x >= 0 && y >= 0) && (x < bitmap.PixelWidth && y < bitmap.PixelHeight))
                        {
                            // Find the address of the pixel to draw.
                            IntPtr pBackBuffer = bitmap.BackBuffer + (int)y * bitmap.BackBufferStride + (int)x * 4;

                            // Assign the color data to the pixel.
                            *((int*)pBackBuffer) = color_data;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                // Specify the area of the bitmap that changed.
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                bitmap.Unlock();
            }
        }

        private static byte[] _blankBuffer;

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
            try
            {
                unsafe
                {
                    // Compute the pixel's color.
                    int color_data = color.ToInt();
                    int pixelWidth = bitmap.PixelWidth;
                    int pixelHight = bitmap.PixelHeight;
                    IntPtr backBuffer = bitmap.BackBuffer;
                    int stride = bitmap.BackBufferStride;

                    Parallel.ForEach(Partitioner.Create(0, polygons.Length), Range =>
                    {
                        for (int i = Range.Item1; i < Range.Item2; i++)
                        {
                            for (int j = 0; j < polygons[i].Vertices.Length; j++)
                            {
                                Point x1;
                                Point x2;
                                if (j < polygons[i].Vertices.Length - 1)
                                {
                                    x1 = vertices[polygons[i].Vertices[j].VertexIndex];
                                    x2 = vertices[polygons[i].Vertices[j + 1].VertexIndex];
                                }
                                else
                                {
                                    x1 = vertices[polygons[i].Vertices[^1].VertexIndex];
                                    x2 = vertices[polygons[i].Vertices[0].VertexIndex];
                                }

                                double x2x1 = x2.X - x1.X;
                                double y2y1 = x2.Y - x1.Y;
                                double l = Math.Abs(x2x1) > Math.Abs(y2y1) ? Math.Abs(x2x1) : Math.Abs(y2y1);
                                double xDelta = x2x1 / l;
                                double yDelta = y2y1 / l;

                                for (int k = 0; k < l; k++)
                                {
                                    double x = x1.X + k * xDelta;
                                    double y = x1.Y + k * yDelta;
                                    if ((x >= 0 && y >= 0) && (x < pixelWidth && y < pixelHight))
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

                    });
                }

                bitmap.Lock();
                // Specify the area of the bitmap that changed.
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
