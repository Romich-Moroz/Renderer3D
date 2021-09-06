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

                    // Compute the pixel's color.
                    int color_data = color.R << 16; // R
                    color_data |= color.G << 8;   // G
                    color_data |= color.B << 0;   // B

                    // Assign the color data to the pixel.
                    *((int*)pBackBuffer) = color_data;
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
            var xDelta = x2x1/l;
            var yDelta = y2y1/l;
            for (int i = 0; i < l; i++)
            {
                if ((x1.X + i * xDelta >= 0 && x1.Y + i * yDelta >= 0) && (x1.X + i * xDelta < bitmap.PixelWidth && x1.Y + i * yDelta < bitmap.PixelHeight))
                {
                    bitmap.DrawPixel(new Point { X = x1.X + i * xDelta, Y = x1.Y + i * yDelta }, color);
                }
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
                    int color_data = color.R << 16; // R
                    color_data |= color.G << 8;   // G
                    color_data |= color.B << 0;   // B

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

    }
}
