using Renderer3D.Models.Data;
using System;
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
        public static void DrawLine(this WriteableBitmap bitmap, Point x1, Point x2, Color color)
        {
            throw new NotImplementedException();
        }

        public static int ToInt(this Color color) => color.R << 16 | color.G << 8 | color.B << 0;

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

        public static void DrawLineDda(this WriteableBitmap bitmap, Point x1, Point x2, Color color)
        {
            var x2x1 = x2.X - x1.X;
            var y2y1 = x2.Y - x1.Y;
            var l = Math.Abs(x2x1) > Math.Abs(y2y1) ? Math.Abs(x2x1) : Math.Abs(y2y1);
            var xDelta = x2x1 / l;
            var yDelta = y2y1 / l;

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
                        var x = x1.X + i * xDelta;
                        var y = x1.Y + i * yDelta;
                        if ((x >= 0 && y>= 0) && (x < bitmap.PixelWidth && y < bitmap.PixelHeight))
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
                bitmap.AddDirtyRect(new Int32Rect(0,0, bitmap.PixelWidth,bitmap.PixelHeight));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                bitmap.Unlock();
            }  
        }

        public static void Clear(this WriteableBitmap bitmap, Color color)
        {
            try
            {
                // Reserve the back buffer for updates.
                bitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = bitmap.BackBuffer;

                    // Compute the pixel's color.
                    int color_data = color.ToInt();

                    for (int i=0; i< bitmap.Width * bitmap.PixelHeight; i++)
                    {
                        // Assign the color data to the pixel.
                        *((int*)pBackBuffer) = color_data;
                        pBackBuffer += 4;
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

        //INLINED FOR OPTIMIZATION PURPOSES
        public static void DrawPolygons(this WriteableBitmap bitmap, Polygon[] polygons, Point[] vertices, Color color)
        {
            try
            {
                // Reserve the back buffer for updates.
                bitmap.Lock();

                unsafe
                {
                    // Compute the pixel's color.
                    int color_data = color.ToInt();

                    for (int i = 0; i < polygons.Length; i++)
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
                                x1 = vertices[polygons[i].Vertices[polygons[i].Vertices.Length - 1].VertexIndex];
                                x2 = vertices[polygons[i].Vertices[0].VertexIndex];
                            }

                            var x2x1 = x2.X - x1.X;
                            var y2y1 = x2.Y - x1.Y;
                            var l = Math.Abs(x2x1) > Math.Abs(y2y1) ? Math.Abs(x2x1) : Math.Abs(y2y1);
                            var xDelta = x2x1 / l;
                            var yDelta = y2y1 / l;

                            for (int k = 0; k < l; k++)
                            {
                                var x = x1.X + k * xDelta;
                                var y = x1.Y + k * yDelta;
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

    }
}
