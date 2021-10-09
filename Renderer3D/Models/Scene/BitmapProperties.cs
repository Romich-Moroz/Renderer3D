using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.Scene
{
    public struct BitmapProperties
    {
        public PixelFormat PixelFormat { get; set; }

        /// <summary>
        /// Width of the bitmap in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of the bitmap in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Width of the row of pixels of the bitmap
        /// </summary>
        public int Stride => (Width * PixelFormat.BitsPerPixel + 7) / 8;

        /// <summary>
        /// Aspect ration of the screen aka Width / Height
        /// </summary>
        public float AspectRatio => (float)Width / Height;

        public BitmapProperties(PixelFormat pixelFormat, int width, int height)
        {
            (PixelFormat, Width, Height) = (pixelFormat, width, height);
        }

        public WriteableBitmap CreateFromProperties()
        {
            return new WriteableBitmap
            (
                BitmapSource.Create
                (
                    Width,
                    Height,
                    96d,
                    96d,
                    PixelFormat,
                    null,
                    new byte[Height * Stride],
                    Stride
                )
            );
        }
    }
}
