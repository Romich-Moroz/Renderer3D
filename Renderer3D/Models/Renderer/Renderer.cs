using Renderer3D.Models.Parser;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.Renderer
{
    /// <summary>
    /// Renders complex models into bitmap source
    /// </summary>
    public class Renderer
    {
        /// <summary>
        /// Format of pixels for rendered bitmap
        /// </summary>
        public PixelFormat PixelFormat { get; set; }
        /// <summary>
        /// Width of the bitmap
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height of the bitmap
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Width of the row of pixels of the bitmap
        /// </summary>
        public int Stride => (Width * PixelFormat.BitsPerPixel + 7) / 8;
        /// <summary>
        /// Parsed model to render on bitmap
        /// </summary>
        public ObjectModel ObjectModel { get; set; }

        public Renderer(PixelFormat pixelFormat, int width, int height, ObjectModel model)
        {
            (PixelFormat, Width, Height, ObjectModel) = (pixelFormat, width, height, model);
        }

        /// <summary>
        /// Renders the loaded model into bitmap
        /// </summary>
        /// <returns>Rendered bitmap</returns>
        public BitmapSource Render()
        {
            //Init bitmap
            WriteableBitmap _bitmap = new WriteableBitmap(BitmapSource.Create(Width, Height, 96d, 96d, PixelFormat, null, new byte[Height * Stride], Stride));

            //Draw logic here...
            //Use _bitmap.ExtensionMethod to draw




            _bitmap.Freeze();
            return _bitmap;
        }
    }
}
